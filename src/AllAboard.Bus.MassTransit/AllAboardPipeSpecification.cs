namespace AllAboard.Bus.MassTransit
{
    using System.Collections.Generic;
    using System.Linq;
    using global::MassTransit;
    using GreenPipes;

    public class AllAboardPipeSpecification : IPipeSpecification<ConsumeContext>, IPipeSpecification<PublishContext>
    {
        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new AllAboardConsumeFilter<ConsumeContext>());
        }

        public void Apply(IPipeBuilder<PublishContext> builder)
        { 
            builder.AddFilter(new AllAboardPublishFilter<PublishContext>());
        }
    }
}