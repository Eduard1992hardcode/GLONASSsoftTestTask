namespace EventBus
{
    public class EventBusOptions
    {
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public ushort PrefetchCount { get; set; } // 1
        public uint PrefetchSize { get; set; } // 0
        public bool Durable { get; set; } // true
        public bool AutoDelete { get; set; } // false
        public bool Exclusive { get; set; } // false
        public bool Global { get; set; } // false
        public byte Priority { get; set; }
    }
}
