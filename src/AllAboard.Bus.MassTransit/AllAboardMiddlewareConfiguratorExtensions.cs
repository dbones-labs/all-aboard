namespace AllAboard.Bus.MassTransit
{
    using global::MassTransit;

    public static class AllAboardMiddlewareConfiguratorExtensions
    {
        public static void UseAllAboard(this IBusFactoryConfigurator value)
        {
            value.ConfigurePublish(configurator => configurator.AddPipeSpecification(new AllAboardPipeSpecification()));
            value.AddPipeSpecification(new AllAboardPipeSpecification());
        }
    }
}