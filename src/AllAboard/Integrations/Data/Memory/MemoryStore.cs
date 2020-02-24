namespace AllAboard.Integrations.Data.Memory
{
    using System.Collections.Generic;
    using Services;

    public class MemoryStore
    {
        public List<Entity> Store { get; set; } = new List<Entity>();
    }
}