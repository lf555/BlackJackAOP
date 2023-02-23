using Microsoft.Extensions.DependencyInjection;

namespace BlackJackAOP
{
    public static class InterceptionBuilderExtensions
    {
        public static InterceptionBuilder RegisterInterceptors(this InterceptionBuilder builder, Action<IInterceptorRegistry> register)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (register == null) throw new ArgumentNullException(nameof(register));
            builder.Services.Configure<InterceptionOptions>(it => it.InterceptorRegistrations = register);
            return builder;
        }
    }
}
