using BlackJackAOP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackAOP.AspNetCore
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseInterception(this IHostBuilder hostBuilder, Action<InterceptionBuilder>? setup = null)
            => UseInterception(hostBuilder, new ServiceProviderOptions(), setup);

        public static IHostBuilder UseInterception(this IHostBuilder hostBuilder, ServiceProviderOptions serviceProviderOptions, Action<InterceptionBuilder>? setup = null)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));
            if (serviceProviderOptions == null) throw new ArgumentNullException(nameof(serviceProviderOptions));
            hostBuilder.ConfigureServices((_, services) => services.AddHttpContextAccessor());
            Action<InterceptionBuilder> configure = builder =>
            {
                builder.Services.Replace(ServiceDescriptor.Singleton<IInvocationServiceScopeFactory, RequestServiceScopeFactory>());
                setup?.Invoke(builder);
            };
            return hostBuilder.UseServiceProviderFactory(new InterceptionServiceProviderFactory(serviceProviderOptions ?? new ServiceProviderOptions(), configure));
        }

        private class InterceptionServiceProviderFactory : IServiceProviderFactory<InterceptableContainerBuilder>
        {
            private readonly Action<InterceptionBuilder>? _setup;
            private readonly ServiceProviderOptions _providerOptions;

            public InterceptionServiceProviderFactory(ServiceProviderOptions providerOptions, Action<InterceptionBuilder>? setup)
            {
                _setup = setup;
                _providerOptions = providerOptions;
            }

            public InterceptableContainerBuilder CreateBuilder(IServiceCollection services) => new(services, _providerOptions, _setup);
            public IServiceProvider CreateServiceProvider(InterceptableContainerBuilder containerBuilder) => containerBuilder.CreateServiceProvider();
        }
    }
}
