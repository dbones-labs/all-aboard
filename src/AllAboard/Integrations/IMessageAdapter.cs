namespace AllAboard.Integrations
{
    using Services;

    public interface IMessageAdapter
    {
        void Populate(object dest, MessageEntry source);
    }
}