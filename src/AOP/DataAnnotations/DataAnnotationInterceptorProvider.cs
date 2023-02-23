﻿using System.Reflection;

namespace BlackJackAOP
{
    [NonInterceptable]
    internal class DataAnnotationInterceptorProvider : InterceptorProviderBase
    {
        private readonly BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public DataAnnotationInterceptorProvider(IConventionalInterceptorFactory interceptorFactory) : base(interceptorFactory)
        {
        }

        public override IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (method == null) throw new ArgumentNullException(nameof(method));

            var declaringType = method.DeclaringType!;
            if (declaringType != targetType)
            {
                return GetInterceptors(declaringType, method);
            }

            var list = new List<Sortable<InvokeDelegate>>();
            foreach (var attribute in targetType.GetCustomAttributes<InterceptorAttribute>(false))
            {
                list.Add(new Sortable<InvokeDelegate>(attribute.Order, CreateInterceptor(attribute)));
            }

            foreach (var attribute in method.GetCustomAttributes<InterceptorAttribute>(false))
            {
                list.Add(new Sortable<InvokeDelegate>(attribute.Order, CreateInterceptor(attribute)));
            }

            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                if (MemberUtilities.TryGetProperty(method, out var property))
                {
                    foreach (var attribute in property!.GetCustomAttributes<InterceptorAttribute>(false))
                    {
                        list.Add(new Sortable<InvokeDelegate>(attribute.Order, CreateInterceptor(attribute)));
                    }
                }
            }
            return list;
        }

        public override void Validate(Type type, Action<MethodInfo> methodValidator, Action<PropertyInfo> propertyValidator)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var isPublic = type.IsPublic || type.IsNestedPublic;
            var attribute = type.GetCustomAttributes<InterceptorAttribute>().FirstOrDefault();
            if (!isPublic && attribute is not null)
            {
                throw new InterceptionException($"{type}必须是公开的类型。");
            }           

            foreach (var method in type.GetMethods(_bindingFlags))
            {
                attribute = method.GetCustomAttributes<InterceptorAttribute>().FirstOrDefault();
                if (attribute is not null)
                {
                    if (!isPublic)
                    {
                        throw new InterceptionException($"{type}必须是公开的类型。");
                    }
                    methodValidator(method);
                }                   
            }

            foreach (var property in type.GetProperties(_bindingFlags))
            {
                attribute = property.GetCustomAttributes<InterceptorAttribute>().FirstOrDefault();
                if (attribute is not null)
                {
                    if (!isPublic)
                    {
                        throw new InterceptionException($"{type}必须是公开的类型。");
                    }
                    propertyValidator(property);
                }
            }
        }

        private readonly Dictionary<Assembly, bool> _suppressedAssemblies = new();
        public override bool CanIntercept(Type targetType, MethodInfo method, out bool suppressed)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (method == null) throw new ArgumentNullException(nameof(method));
            PropertyInfo? property = null;

            var assembly = targetType.Assembly;
            if (_suppressedAssemblies.TryGetValue(assembly, out var sup) && sup)
            {
                suppressed = true;
                return false;
            }
            else
            {
                sup = assembly.GetCustomAttributes<NonInterceptableAttribute>().Any();
                _suppressedAssemblies[assembly] = sup;
                if (sup)
                {
                    suppressed = true;
                    return false;
                }
            }

            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                if (MemberUtilities.TryGetProperty(method, out property) && property!.GetCustomAttributes<NonInterceptableAttribute>().Any())
                {
                    suppressed = true;
                    return false;
                }
            }

            if (targetType.GetCustomAttributes<NonInterceptableAttribute>().Any() || method.GetCustomAttributes<NonInterceptableAttribute>().Any())
            {
                suppressed = true;
                return false;
            }

            suppressed = false;
            if (method.DeclaringType == targetType)
            {
                return method.GetCustomAttributes<InterceptorAttribute>(false).Any() || targetType.GetCustomAttributes<InterceptorAttribute>(false).Any() || (property?.GetCustomAttributes(false)?.Any() ?? false);
            }

            return CanIntercept(method.DeclaringType!, method, out suppressed);
        }

        private InvokeDelegate CreateInterceptor(InterceptorAttribute interceptorAttribute)
            => interceptorAttribute.Interceptor == interceptorAttribute.GetType()
            ? InterceptorFactory.CreateInterceptor(interceptorAttribute)
            : InterceptorFactory.CreateInterceptor(interceptorAttribute.Interceptor, interceptorAttribute.Arguments);
    }
}
