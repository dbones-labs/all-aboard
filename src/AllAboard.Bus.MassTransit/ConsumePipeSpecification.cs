namespace AllAboard.Bus.MassTransit
{
    using System.Collections.Generic;
    using System.Linq;
    using global::MassTransit;
    using GreenPipes;

    public class ConsumePipeSpecification : IPipeSpecification<ConsumeContext>
    {
        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new ConsumeFilter<ConsumeContext>());
        }
    }
}