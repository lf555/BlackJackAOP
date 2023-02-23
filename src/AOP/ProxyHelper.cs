using System.Reflection;

namespace BlackJackAOP
{
    public static class ProxyHelper
    {
        private static readonly Dictionary<Tuple<Type, int>, MethodInfo> _methods = new();
        private static readonly BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static TOutput GetArgumentOrReturnValue<TAugument, TOutput>(TAugument value)
        {
            if (value is TOutput result)
            {
                return result;
            }

            if (typeof(TOutput).IsAssignableFrom(typeof(TAugument)))
            {
                return default!;
            }

            throw new InvalidCastException($"无法强制转换参数值从{typeof(TAugument)}到{typeof(TOutput)}");
        }

        public static TInvocationContext SetArgumentOrReturnValue<TInvocationContext, TArgument, TInput>(TInvocationContext context, TInput value, Action<TInvocationContext, TArgument> evaluate) where TInvocationContext : InvocationContext
        {
            if (value is TArgument argument)
            {
                evaluate(context, argument);
                return context;
            }

            if (typeof(TArgument).IsAssignableFrom(typeof(TInput)))
            {
                evaluate(context, default!);
                return context;
            }

            throw new InvalidCastException($"无法强制转换参数值从{typeof(TInput)}到{typeof(TArgument)}");
        }

        public static MethodInfo GetMethodInfo<T>(int metadataToken)
        {
            var key = new Tuple<Type, int>(typeof(T), metadataToken);
            return _methods.TryGetValue(key, out var method)
                ? method
                : _methods[key] = typeof(T).GetMethods(_bindingFlags).FirstOrDefault(it => it.MetadataToken == metadataToken) ?? throw new ArgumentException($"在{typeof(T)}类型中找不到由指定令牌标识的方法");
        }

        public static async ValueTask AsValueTask<TResult>(this ValueTask<TResult> valueTask)
        {
            if (!valueTask.IsCompleted)
            {
                await valueTask;
            }
        }
    }
}
