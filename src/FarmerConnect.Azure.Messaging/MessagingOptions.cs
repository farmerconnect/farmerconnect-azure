namespace FarmerConnect.Azure.Messaging
{
    public class MessagingOptions
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public int MaxMessages { get; set; } = 1; // Maximum number of messages to receive at once. If set to one then the behaviour is more similar to the service bus implementation with pub / sub.
        public int MaxPollingInterval { get; set; } = 51200; // This is the maximum polling interval. Since we pay connections to the storage account this could lead to unwanted cost if not high enough.
    }
}
