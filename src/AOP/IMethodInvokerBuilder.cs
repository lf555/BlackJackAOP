using System.Reflection;

namespace BlackJackAOP
{
    public interface IMethodInvokerBuilder
    {
        InvokeDelegate Build(Type targetType, MethodInfo method, InvokeDelegate targetMethodInvoker);

        bool CanIntercept(Type targetType, MethodInfo method);
    }
}
