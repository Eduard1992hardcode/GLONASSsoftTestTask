using EventBus;
using EventBus.Abstactions;
using EventBusRabbitMQ;
using GLONASSsoftTestTask.EventHandlers;
using GLONASSsoftTestTask.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace GLONASSsoftTestTask.Infrastructure.DependencyInjections
{
    public static class RabbitDependencyInjections
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<EventBusOptions>();
            services.Configure<EventBusOptions>(configuration.GetSection(nameof(EventBusOptions)));

            services.AddSingleton<IEventBus, EventBusRabbitMQ.EventBusRabbitMQ>();
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            var configSection = configuration.GetSection("RabbitMQConnection");

            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var factory = new ConnectionFactory()
                {
                    HostName = configSection.GetValue<string>("HostName"),
                    DispatchConsumersAsync = true,
                    Port = configSection.GetValue<int>("Port"),
                    UserName = configSection.GetValue<string>("UserName"),
                    Password = configSection.GetValue<string>("Password"),
                    VirtualHost = configSection.GetValue<string>("VirtualHost")
                };
                return new DefaultRabbitMQPersistentConnection(
                    factory,
                    logger,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name);
            });

            services.AddTransient<CreateTaskStatisticEventHandler>();
            

            return services;
        }

        public static IApplicationBuilder ConfigureRabbitMQ(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<CreateTaskStatisticEvent, CreateTaskStatisticEventHandler>();
            
            return app;
        }
    }
}
