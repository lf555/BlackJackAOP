using Microsoft.Extensions.DependencyInjection;

namespace BlackJackAOP
{
    public sealed class InterceptionBuilder
    {
        public InterceptionBuilder(IServiceCollection services)=> Services = services ?? throw new ArgumentNullException(nameof(services));

        public IServiceCollection Services { get; }
    }
}
