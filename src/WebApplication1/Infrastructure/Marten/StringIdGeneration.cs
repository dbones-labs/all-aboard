namespace WebApplication1.Infrastructure.Marten
{
    using System;
    using System.Collections.Generic;
    using global::Marten.Schema.Identity;
    using global::Marten.Storage;

    public class StringIdGeneration : IIdGeneration
    {

        public IEnumerable<System.Type> KeyTypes { get; } = new System.Type[] { typeof(string) };

        public IIdGenerator<T> Build<T>()
        {
            return (IIdGenerator<T>)new StringIdGenerator();
        }

        public bool RequiresSequences { get; } = false;

        public class StringIdGenerator : IIdGenerator<string>
        {
            public string Assign(ITenant tenant, string existing, out bool assigned)
            {
                assigned = true;
                return string.IsNullOrWhiteSpace(existing)
                    ? ToShortString(Guid.NewGuid())
                    : existing;
            }

            public string ToShortString(Guid guid)
            {
                var base64Guid = Convert.ToBase64String(guid.ToByteArray());

                // Replace URL unfriendly characters with better ones
                base64Guid = base64Guid.Replace('+', '-').Replace('/', '_');

                // Remove the trailing ==
                return base64Guid.Substring(0, base64Guid.Length - 2);
            }
        }
    }
}