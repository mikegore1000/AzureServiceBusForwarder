namespace AzureServiceBusForwarder
{
    public class Metric
    {
        public Metric(string name, int value)
        {
            Name = name;
            Value = value;
        }
        
        public string Name { get; }
        
        public int Value { get; }
    }
}