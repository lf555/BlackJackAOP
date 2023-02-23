using System.Linq.Expressions;
using System.Reflection;

namespace BlackJackAOP
{
    public interface IInterceptorRegistry<TInterceptor>
    {
        IInterceptorRegistry<TInterceptor> ToAllMethods<TTarget>(int order);

        IInterceptorRegistry<TInterceptor> ToMethod<TTarget>(int order, Expression<Action<TTarget>> methodCall);

        IInterceptorRegistry<TInterceptor> ToMethods<TTarget>(int order, params MethodInfo[] methods);

        IInterceptorRegistry<TInterceptor> ToGetMethod<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor);

        IInterceptorRegistry<TInterceptor> ToSetMethod<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor);

        IInterceptorRegistry<TInterceptor> ToProperty<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor);
    }
}
