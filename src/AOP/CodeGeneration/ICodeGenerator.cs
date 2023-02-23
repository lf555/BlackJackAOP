using Microsoft.Extensions.DependencyInjection;

namespace BlackJackAOP
{
    public interface ICodeGenerator
    {
        bool TryGenerate(ServiceDescriptor serviceDescriptor, CodeGenerationContext codeGenerationContext, out string[]? proxyTypeNames);

        void RegisterProxyType(IServiceCollection services, ServiceDescriptor serviceDescriptor, Type[] proxyTypes);
    }
}
