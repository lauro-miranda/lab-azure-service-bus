namespace LAB.AzureServiceBus.Messages
{
    public class Message
    {
        public Guid Id { get; set; }

        public int Total { get; set; }

        public int Current { get; set; }

        public string Value { get; set; } = "";
    }
}