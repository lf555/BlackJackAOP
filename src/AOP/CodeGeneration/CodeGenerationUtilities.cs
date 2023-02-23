﻿using System.Reflection;

namespace BlackJackAOP
{
    internal static class CodeGenerationUtilities
    {
        public static string GetOutputName(this Type type)
        {
            if (type == typeof(void))
            {
                return "void";
            }
            var fullName = type.IsGenericParameter? type.Name: type.FullName??$"{type.Namespace}.{type.Name}".TrimStart('.');
            if (type.IsGenericType)
            {
                var arguments = type.GetGenericArguments().Select(it => GetOutputName(it));
                fullName = $"{fullName.Substring(0, fullName.IndexOf('`'))}<{string.Join(", ", arguments).Trim()}>";
            }

            return fullName.Replace("+", ".").TrimEnd('&');          
        }

        public static string NextSurfix => Math.Abs( Guid.NewGuid().GetHashCode()).ToString();

        public static string AsIdentifier(this string baseName) => $"{baseName}{Math.Abs(Guid.NewGuid().GetHashCode())}";

        public static string GetModifier(this MethodBase methodInfo)
        {
            if (methodInfo.IsPublic)
            {
                return "public";
            }

            if (methodInfo.IsPrivate)
            {
                return "private";
            }

            if (methodInfo.IsFamilyAndAssembly)
            {
                return "internal protected";
            }

            if (methodInfo.IsFamily)
            {
                return "protected";
            }

            return "internal";
        }

        public static string GetModifier(this PropertyInfo property)
        {
            if ( (property?.GetMethod?.IsPublic ?? false) || (property?.SetMethod?.IsPublic ?? false))
            {
                return "public";
            }

            if ((property?.GetMethod?.IsFamilyAndAssembly ?? false) || (property?.SetMethod?.IsFamilyAndAssembly ?? false))
            {
                return "internal protected";
            }

            if ((property?.GetMethod?.IsFamily ?? false) || (property?.SetMethod?.IsFamily ?? false))
            {
                return "protected";
            }

            return "internal";
        }

        public static bool IsAwaitable(this Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            var definition = type.GetGenericTypeDefinition();
            return definition == typeof(Task<>) || definition == typeof(ValueTask<>);
        }

        public static ReturnKind AsReturnKind(this Type type)
        {
            if (type == typeof(void))
            {
                return ReturnKind.Void;
            }

            if (type == typeof(ValueTask))
            {
                return ReturnKind.ValueTask;
            }

            if (type == typeof(Task))
            {
                return ReturnKind.Task;
            }

            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                if (definition == typeof(ValueTask<>))
                {
                    return ReturnKind.ValueTaskOfResult;
                }
                if (definition == typeof(Task<>))
                {
                    return ReturnKind.TaskOfResult;
                }
            }
            return ReturnKind.Result;
        }

        public static void EnsureMethodInterceptable(MethodInfo method)
        {
            if (!MemberUtilities.IsInterfaceOrVirtualMethod(method))
            {
                throw new InterceptionException($"不能对{method.DeclaringType}的{method.Name}方法进行拦截，必须是虚方法或者接口的实现方法。");
            }
        }

        public static void EnsurePropertyInterceptable(PropertyInfo  property)
        {
            var method = property.GetMethod;
            if (method is not null && MemberUtilities.IsInterfaceOrVirtualMethod(method))
            {
                return;
            }
            method = property.SetMethod;
            if (method is not null && MemberUtilities.IsInterfaceOrVirtualMethod(method))
            {
                return;
            }
            throw new InterceptionException($"不能对{property.DeclaringType}的{property.Name}属性进行拦截，其get或set访问器必须是虚方法或者接口的实现方法");
        }
    }

    internal enum ReturnKind
    { 
        Void,
        Result,
        Task,
        TaskOfResult,
        ValueTask,
        ValueTaskOfResult,
    }
}
