using EventBus;
using EventBus.Abstactions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBusRabbitMQ
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly string _exchangeName;
        private readonly EventBusOptions _consumerInfo;

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _retryCount;

        private IModel _consumerChannel;

        private string _queueName;

        bool _disposed;

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger,
            IEventBusSubscriptionsManager subsManager, IOptions<EventBusOptions> options,
            IServiceProvider serviceProvider, int retryCount = 0)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();

            _consumerInfo = options.Value;
            _queueName = _consumerInfo.QueueName;
            _exchangeName = _consumerInfo.ExchangeName;

            _serviceProvider = serviceProvider;

            _retryCount = retryCount;
            _consumerChannel = CreateConsumerChannel();
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
            StartBasicConsume();
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName,
                    exchange: _exchangeName,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }


        public void Publish(IEnumerable<IntegrationEvent> events)
        {
            using (var channel = _persistentConnection.CreateModel())
            {
                var batch = channel.CreateBasicPublishBatch();
                var eventName = events.First().GetType().Name;
                channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct, durable: true);

                foreach (var @event in events)
                {
                    _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message).AsMemory();

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;
                    properties.Priority = @event.Priority;

                    batch.Add(exchange: _exchangeName,
                                     routingKey: @event.RoutingKey ?? eventName,
                                     mandatory: true,
                                     properties: properties,
                                     body: body);
                }

                batch.Publish();
            }
        }


        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
                });

            var eventName = @event.GetType().Name;

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);


            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

                channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct, durable: true);

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;
                    if (properties.Priority != 0)
                    {
                        properties.Priority = @event.Priority;
                    }

                    properties.Headers = new Dictionary<string, object> { { "CountErrors", 0 } };

                    _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                    channel.BasicPublish(exchange: _exchangeName,
                                     routingKey: @event.RoutingKey ?? eventName,
                                     mandatory: true,
                                     basicProperties: properties,
                                     body: body);
                });
            }
        }

        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

            DoInternalSubscription(eventName);
            _subsManager.AddDynamicSubscription<TH>(eventName);
            StartBasicConsume();

        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

            _subsManager.AddSubscription<T, TH>();

        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(queue: _queueName,
                                      exchange: _exchangeName,
                                      routingKey: eventName);
                }
            }
        }

        public void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            var eventName = _subsManager.GetEventKey<T>();

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }


        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            _subsManager.Clear();
        }

        private void StartBasicConsume()
        {

            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {

                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_ReceivedAsync;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);

                _consumerChannel.CallbackException += ConsumerChannel_CallbackException;

                consumer.ConsumerCancelled += Consumer_ConsumerCancelledAsync;
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        private void ConsumerChannel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        }

        private async void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body.Span);

            _logger.LogDebug(" Recivent RabbitMQ event: \"{EventName}\"", eventName);

            try
            {
                var result = await ProcessEvent(eventName, message);
                if (result)
                {
                    _consumerChannel.BasicAck(e.DeliveryTag, false);
                }
                else
                {
                    _consumerChannel.BasicNack(e.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
                // Even on exception we take the message off the queue.
                // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
                // For more information see: https://www.rabbitmq.com/dlx.html
                _consumerChannel.BasicNack(e.DeliveryTag, false, true);
            }
        }

        private async Task Consumer_ConsumerCancelledAsync(object sender, ConsumerEventArgs @event)
        {
            if (_disposed) return;

            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            _logger.LogDebug(" Recivent RabbitMQ event: \"{EventName}\"", eventName);

            try
            {
                var result = await ProcessEvent(eventName, message);
                if (result)
                {
                    _consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    _consumerChannel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
                // Even on exception we take the message off the queue.
                // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
                // For more information see: https://www.rabbitmq.com/dlx.html
                _consumerChannel.BasicNack(eventArgs.DeliveryTag, false, true);
            }
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ consumer channel");

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct, durable: true);

            channel.BasicQos(
                    prefetchSize: _consumerInfo.PrefetchSize,
                    prefetchCount: _consumerInfo.PrefetchCount,
                    global: _consumerInfo.Global
             );



            channel.QueueDeclare(
                    queue: _queueName,
                    durable: _consumerInfo.Durable,
                    autoDelete: _consumerInfo.AutoDelete,
                    exclusive: _consumerInfo.Exclusive,
                    arguments: _consumerInfo.Priority > 0
                        ? new Dictionary<string, object>() { { "x-max-priority", _consumerInfo.Priority } }
                        : null
            );

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };


            return channel;
        }

        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            bool result = false;

            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                        result = await ((Task<bool>)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent }));
                        _logger.LogTrace(" Processing RabbitMQ event: {EventName}", eventName);

                    }
                }
            }
            else
            {
                _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
            }

            return result;
        }
    }
}
