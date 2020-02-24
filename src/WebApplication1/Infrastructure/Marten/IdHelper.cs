namespace WebApplication1.Infrastructure.Marten
{
    using System;

    public static class IdHelper
    {
        public static string GenerateId()
        {
            var base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // Replace URL unfriendly characters with better ones
            base64Guid = base64Guid.Replace('+', '-').Replace('/', '_');

            // Remove the trailing ==
            return base64Guid.Substring(0, base64Guid.Length - 2);
        }
    }
}