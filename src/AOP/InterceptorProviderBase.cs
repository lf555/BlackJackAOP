using System.Reflection;

namespace BlackJackAOP
{
    public abstract class InterceptorProviderBase : IInterceptorProvider
    {
        protected InterceptorProviderBase(IConventionalInterceptorFactory interceptorFactory) => InterceptorFactory = interceptorFactory ?? throw new ArgumentNullException(nameof(interceptorFactory));

        public IConventionalInterceptorFactory InterceptorFactory { get; }

        public abstract bool CanIntercept(Type targetType, MethodInfo method, out bool suppressed);

        public abstract IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method);

        public virtual void Validate(Type targetType, Action<MethodInfo> methodValidator, Action<PropertyInfo> propertyValidator) { }
    }
}
