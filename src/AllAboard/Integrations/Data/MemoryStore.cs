namespace AllAboard.Integrations.Data
{
    using System.Collections.Generic;
    using Services;

    public class MemoryStore
    {
        public List<Entity> Store { get; set; } = new List<Entity>();
    }
}