﻿using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

namespace BlackJackAOP
{
    [NonInterceptable]
    internal sealed class ConventionalInterceptorFactory : IConventionalInterceptorFactory
    {
        #region Fields
        private readonly object[] _emptyArugments = Array.Empty<object>();
        private readonly IApplicationServicesAccessor _applicationServicesAccessor;
        private readonly IServiceLifetimeProvider _serviceLifetimeProvider;
        private static readonly Dictionary<Type, Func<object, InvocationContext, ValueTask>> _invokers = new();
        private static readonly object _syncHelper = new();
        private static readonly MethodInfo _getRequiredServiceMethod = MemberUtilities.GetMethod(() => GetRequiredService<int>(default!)).GetGenericMethodDefinition();
        #endregion

        #region Constructors
        public ConventionalInterceptorFactory(IApplicationServicesAccessor  applicationServicesAccessor, IServiceLifetimeProvider serviceLifetimeProvider)
        {
            _applicationServicesAccessor = applicationServicesAccessor ?? throw new ArgumentNullException(nameof(applicationServicesAccessor));
            _serviceLifetimeProvider = serviceLifetimeProvider ?? throw new ArgumentNullException(nameof(serviceLifetimeProvider));
        }
        #endregion

        #region Public methods
        public InvokeDelegate CreateInterceptor(Type interceptorType, params object[] arguments)
        {
            Guard.ArgumentNotNull(interceptorType);
            EnsourceValidInterceptorType(interceptorType, arguments, out var invokeAsyncMethod);
            object instance = ActivatorUtilities.CreateInstance(_applicationServicesAccessor.ApplicationServices, interceptorType, arguments);
            if (!TryGetInvoke(interceptorType, invokeAsyncMethod, out var invoker))
            {
                throw new ArgumentException($"指定的拦截器无效。", nameof(interceptorType));
            }
            return context => invoker!(instance, context);         
        }

        public InvokeDelegate CreateInterceptor(object interceptor)
        {
            Guard.ArgumentNotNull(interceptor);
            var interceptorType = interceptor.GetType();
            EnsourceValidInterceptorType(interceptorType, _emptyArugments, out var invokeAsyncMethod);
            if (!TryGetInvoke(interceptorType, invokeAsyncMethod, out var invoker))
            {
                throw new ArgumentException($"指定的拦截器无效。", nameof(interceptorType));
            }
            return context => invoker!(interceptor, context);
        }
        #endregion

        #region Private methods
        private void EnsourceValidInterceptorType(Type interceptorType, object[] arguments, out MethodInfo invokeMethod)
        {
            if (interceptorType.IsAbstract && interceptorType.IsSealed)
            {
                throw new ArgumentException("不能是抽象类或密封类。", nameof(interceptorType));
            }

            var constructors = interceptorType.GetConstructors();
            foreach (var constructor in constructors)
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    var parameterType = parameter.ParameterType;
                    if (arguments.Any(it => parameterType.IsAssignableFrom(it.GetType())))
                    {
                        continue;
                    }
                    //if (_serviceLifetimeProvider.GetLifetime(parameterType) == ServiceLifetime.Scoped)
                    //{
                    //    throw new InterceptionException($"生命周期为scope类型的服务{parameterType}不能注册到拦截器的构造函数中。");
                    //}
                }
            }

            var methods = interceptorType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(it=>it.Name == "InvokeAsync")
                .Where(IsValid)
                .ToArray();

            if (!methods.Any())
            {
                throw new ArgumentException($"指定的类型没有实现InvokeAsync方法。", nameof(interceptorType));
            }

            if (methods.Length > 1)
            {
                throw new ArgumentException($"指定的类型存在多个InvokeAsync方法，无法匹配。.", nameof(interceptorType));
            }

            invokeMethod = methods[0];

            static bool IsValid(MethodInfo method)
            {
                var returnType = method.ReturnType;
                if ( returnType != typeof(ValueTask))
                {
                    return false;
                }

                if (!method.GetParameters().Any(it => it.ParameterType == typeof(InvocationContext)))
                {
                    return false;
                }

                return true;
            }  
        }
        private static T GetRequiredService<T>(InvocationContext invocationContext) where T : notnull => invocationContext.InvocationServices.GetRequiredService<T>();      
        private static bool TryGetInvoke(Type interceptorType, MethodInfo invokeAsyncMethod, out Func<object, InvocationContext, ValueTask>? invoker)
        {
            if (_invokers.TryGetValue(interceptorType, out invoker))
            {
                return true;
            }

            lock (_syncHelper)
            {
                if (_invokers.TryGetValue(interceptorType, out invoker))
                {
                    return true;
                }

                ParameterExpression interceptor = Expression.Parameter(typeof(object), "interceptor");
                ParameterExpression invocationContext = Expression.Parameter(typeof(InvocationContext), "invocationContext");
                var arguments = invokeAsyncMethod.GetParameters().Select(it => GetArgument(invocationContext, it.ParameterType));
                Expression instance = Expression.Convert(interceptor, interceptorType);
                var invoke = Expression.Call(instance, invokeAsyncMethod, arguments);
                invoker = Expression.Lambda<Func<object, InvocationContext, ValueTask>>(invoke, interceptor, invocationContext).Compile();
                _invokers[interceptorType] = invoker;
            }
            return true;
        }
        private static Expression GetArgument(Expression invocationContext, Type parameterType)
        {
            if (parameterType == typeof(InvocationContext))
            {
                return invocationContext;
            }
            Expression callGetService = Expression.Call(_getRequiredServiceMethod.MakeGenericMethod(parameterType), invocationContext);
            return Expression.Convert(callGetService, parameterType);
        }    
        #endregion
    }
}
