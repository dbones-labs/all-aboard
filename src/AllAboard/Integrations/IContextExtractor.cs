namespace AllAboard.Integrations
{
    using Services;

    public interface IContextExtractor
    {
        void Populate(MessageEntry entry, ProcessingContext context);
    }
}