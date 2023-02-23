using System.Linq.Expressions;
using System.Reflection;

namespace BlackJackAOP
{
    public interface IInterceptorRegistry
    {
        IInterceptorRegistry<TInterceptor> For<TInterceptor>(params object[] arguments);

        IInterceptorRegistry SupressType<TTarget>();

        IInterceptorRegistry SupressTypes(params Type[] types);

        IInterceptorRegistry SupressMethod<TTarget>(Expression<Action<TTarget>> methodCall);

        IInterceptorRegistry SupressMethods(params MethodInfo[] methods);

        IInterceptorRegistry SupressProperty<TTarget>(Expression<Func<TTarget, object?>> propertyAccessor);

        IInterceptorRegistry SupressSetMethod<TTarget>(Expression<Func<TTarget, object?>> propertyAccessor);

        IInterceptorRegistry SupressGetMethod<TTarget>(Expression<Func<TTarget, object?>> propertyAccessor);
    }
}
