using System.Linq.Expressions;
using System.Reflection;

namespace BlackJackAOP
{
    public static class MemberUtilities
    {
        public static MethodInfo GetMethod(Expression<Action> methodCallExpression) => ((MethodCallExpression)methodCallExpression.Body).Method;

        public static MethodInfo GetMethod<T>(Expression<Action<T>> methodCallExpression)
        {
            if (methodCallExpression.Body is MethodCallExpression methodCall)
            { 
                var method = methodCall.Method;
                return typeof(T).GetMethod(method.Name,method.GetParameters().Select(it=>it.ParameterType).ToArray())!;
            }
            throw new ArgumentException("指定的表达式不是描述方法的表达式。", nameof(methodCallExpression));
        }

        public static PropertyInfo GetProperty<TTarget>(Expression<Func<TTarget, object?>> propertyAccessExpression)
        {
            // Property
            if (propertyAccessExpression.Body is MemberExpression  memberExpression && memberExpression.Member is PropertyInfo property)
            {
                return property;
            }
           
            //Convert
            if (propertyAccessExpression.Body is UnaryExpression  unaryExpression && unaryExpression.Operand is MemberExpression memberExp && memberExp.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo;
            }

            throw new ArgumentException("指定的表达式不是描述属性的表达式。", nameof(propertyAccessExpression));
        }

        public static bool IsInterfaceOrVirtualMethod(MethodInfo method)
        { 
            if(method.IsVirtual)
            {
                return true;
            }

            var type = method.DeclaringType!;
            foreach (var @interface in type.GetInterfaces())
            {
                var mapping = type.GetInterfaceMap(@interface);
                if (mapping.TargetMethods.Contains(method))
                {
                    return true;
                }
            }
            return false;
        }

        private static Dictionary<MethodInfo, PropertyInfo> _propertyMap = new();

        public static bool TryGetProperty(MethodInfo method, out PropertyInfo? propertyInfo)
        {
            if (_propertyMap.TryGetValue(method, out var property))
            {
                propertyInfo = property;
                return true;
            }

            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                var propertyName = method.Name[4..];
                propertyInfo = method.DeclaringType!.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(it => it.Name == propertyName && (it.GetMethod == method || it.SetMethod == method));
                if (propertyInfo != null)
                {
                    _propertyMap[method] = propertyInfo;
                    return true;
                }
            }
            propertyInfo = null;
            return false;
        }
    }
}
