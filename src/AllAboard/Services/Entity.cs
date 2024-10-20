namespace AllAboard.Services
{
    using System;

    public abstract class Entity
    {
        public string Id { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public Type TopicType { get; set; }
    }
}