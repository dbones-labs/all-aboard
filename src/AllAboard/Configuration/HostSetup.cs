namespace AllAboard.Configuration
{
    public class HostSetup
    {
        internal IFactory DataStoreProvider { get; set; }
        internal IFactory MessagingProvider { get; set; }

        public void UseDataStore<T>() where T : IDataStoreProvider, new()
        {
            DataStoreProvider = new T();
        }

        public void UseMessaging<T>() where T : IMessagingProvider, new()
        {
            MessagingProvider = new T();
        }
    }
}