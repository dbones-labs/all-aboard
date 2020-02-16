namespace AllAboard
{
    /// <summary>
    /// use this in your application to publish messages
    /// </summary>
    public interface IBus
    {
        /// <summary>
        /// publish a message to the message broker
        /// </summary>
        /// <typeparam name="T">message type to publish</typeparam>
        /// <param name="message">message instance to publish</param>
        void Publish<T>(T message);
    }
}