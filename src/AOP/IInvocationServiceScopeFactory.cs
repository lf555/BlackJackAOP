using Microsoft.Extensions.DependencyInjection;

namespace BlackJackAOP
{
    public interface IInvocationServiceScopeFactory
    {
        IServiceScope CreateInvocationScope();
    }
}
