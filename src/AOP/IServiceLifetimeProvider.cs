using Microsoft.Extensions.DependencyInjection;

namespace BlackJackAOP
{
    public interface IServiceLifetimeProvider
    {
        ServiceLifetime? GetLifetime(Type serviceType);
    }
}
