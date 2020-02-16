namespace AllAboard.Services
{
    using System.Collections.Generic;

    /// <summary>
    /// this is the message which will be eventually published onto the bus
    /// </summary>
    public class MessageEntry : Entity
    {
        public string CorrelationId  { get; set; }
        public string SourceId { get; set; }
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public object Body  { get; set; }
        
    }
}