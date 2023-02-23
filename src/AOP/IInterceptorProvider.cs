using System.Reflection;

namespace BlackJackAOP
{
    public interface IInterceptorProvider
    {
        bool CanIntercept(Type targetType, MethodInfo method, out bool suppressed);

        IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method);

        void Validate(Type targetType, Action<MethodInfo> methodValidator, Action<PropertyInfo> propertyValidator);
    }
}
