namespace AllAboard.Integrations.Bus
{
    public interface IIdStrategy
    {
        object NewId();
        object ConvertToProvider(string value);
        string ConvertFromProvider(object value);
    }
}