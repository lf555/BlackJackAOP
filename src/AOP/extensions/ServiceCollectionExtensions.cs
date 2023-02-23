using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlackJackAOP
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder>? setup = null)
        {
            Guard.ArgumentNotNull(services);
            services.AddOptions();
            services.AddLogging();
            services.TryAddSingleton<IApplicationServicesAccessor, ApplicationServicesAccessor>();
            services.TryAddSingleton<IInvocationServiceScopeFactory, InvocationServiceScopeFactory>();
            services.TryAddSingleton<IMethodInvokerBuilder, DefaultMethodInvokerBuilder>();
            services.TryAddSingleton<IConventionalInterceptorFactory, ConventionalInterceptorFactory>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ICodeGenerator, InterfaceProxyGenerator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ICodeGenerator, VirtualMethodProxyGenerator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptorProvider, DataAnnotationInterceptorProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptorProvider, ExpressionInterceptorProvider>());
            setup?.Invoke(new InterceptionBuilder(services));

            return services;
        }

        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, Action<InterceptionBuilder>? setup = null)
            => BuildInterceptableServiceProvider(services, new ServiceProviderOptions(), setup);

        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, ServiceProviderOptions serviceProviderOptions, Action<InterceptionBuilder>? setup = null)
        {
            Guard.ArgumentNotNull(services);
            Guard.ArgumentNotNull(serviceProviderOptions);
            var factgory = new InterceptableServiceProviderFactory(serviceProviderOptions, setup);
            var builder = factgory.CreateBuilder(services);
            return builder.CreateServiceProvider();
        }
    }
}
