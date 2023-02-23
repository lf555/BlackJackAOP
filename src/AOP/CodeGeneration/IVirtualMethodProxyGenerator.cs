using System.Reflection;

namespace BlackJackAOP
{
    interface IVirtualMethodProxyGenerator
    {
        string Generate(CodeGenerationContext codeGenerationContext, Type baseType, MethodInfo[] interceptableMethods);
    }
}
