namespace AllAboard.Bus.MassTransit
{
    using System;
    using Integrations.Bus;
    using Services;

    public class IdStrategy : IIdStrategy
    {
        public object NewId()
        {
            return Guid.NewGuid();
        }

        public object ConvertToProvider(string value)
        {
            if (value == null) return null;
            if (Guid.TryParse(value, out var result)) return result;
            throw new Exception($"sorry {value} is not a valid GUID for MassTransit");
        }

        public string ConvertFromProvider(object value)
        {
            return ((Guid?) value)?.ToString("D");
        }
    }
}