using BlackJackAOP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class PrintReturnValueInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();

            var returnValue = context.GetReturnValue<int>();
            Console.WriteLine($"{context.MethodInfo.Name}拿到的返回值：{returnValue}");
        }
    }

    internal class SetReturnValueInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();

            var returnValue = context.GetReturnValue<string>();
            Console.WriteLine($"{context.MethodInfo.Name}拿到的返回值：{returnValue}");
            context.SetReturnValue(returnValue+" world");
        }
    }

    internal class PrintParametersInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();

            var args=context.MethodInfo.GetParameters();
            Console.WriteLine($"{context.MethodInfo.Name}拿到的参数：");
            Array.ForEach(args, arg =>
            {
                var getArgument= context.GetType().GetMethods().First(it=>it.Name== "GetArgument"&&it.GetParameters()[0].ParameterType==typeof(string)).MakeGenericMethod(arg.ParameterType);
                var result= getArgument.Invoke(context,new object[] {arg.Name! });
                Console.WriteLine($"{arg.Name}:{result}");
            });
        }
    }

    internal class SetParametersInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            var value= context.GetArgument<int>(0);
            Console.WriteLine($"{context.MethodInfo.Name}拿到的参数值：{value}");
            context.SetArgument(0, 100);
            await context.ProceedAsync();
        }
    }

    internal class BarInterceptor1
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            Console.WriteLine($"BarInterceptor1:{context.MethodInfo.Name} before");
            await context.ProceedAsync();
            Console.WriteLine($"BarInterceptor1:{context.MethodInfo.Name} end");
        }
    }

    internal class BarInterceptor2
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            Console.WriteLine($"BarInterceptor2:{context.MethodInfo.Name} before");
            await context.ProceedAsync();
            Console.WriteLine($"BarInterceptor2:{context.MethodInfo.Name} end");
        }
    }
}
