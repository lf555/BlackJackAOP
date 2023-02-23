using BlackJackAOP;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class Compose
    {
        [Fact]
        public void T1()
        {
            var service = new ServiceCollection()
                .AddSingleton<Bar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Bar>();

            var result = service.VirtualMethod(1);
            Assert.True(result=="2");
        }

        [Fact]
        public void T2()
        {
            var service = new ServiceCollection()
                .AddSingleton<IBar,Bar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IBar>();

            var result=service.InterfaceMethod(1);
            Assert.True(result == "2");
        }

        [Fact]
        public void T3()
        {
            var provider = new ServiceCollection()
                .AddSingleton<Bar>()
                .AddSingleton<IBar, Bar>()
                .BuildInterceptableServiceProvider();

            var @interface = provider.GetRequiredService<IBar>();
            var service = provider.GetRequiredService<Bar>();

            var result1=@interface.InterfaceMethod(1);
            var result2=service.VirtualMethod(1);

            Assert.True(result1==result2&&result1=="2");
        }

        public static int num;
        [Fact]
        public void T4()
        {
            var service = new ServiceCollection()
                .AddSingleton<IFooBar, FooBar>()
                .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors))
                .GetRequiredService<IFooBar>();

            service.Method1();
            service.Method2();
            service.Method3();

            Assert.Equal(1,num);
            
        }

        static void RegisterInterceptors(IInterceptorRegistry registry)
        {
            registry.SupressMethod<FooBar>(it=>it.Method1());
        }
    }

    public interface IBar
    {
        string InterfaceMethod(int num);
    }

    public class Bar : IBar
    {
        [Interceptor(typeof(BarInterceptor1))]
        public virtual string InterfaceMethod(int id) => id.ToString();

        [Interceptor(typeof(BarInterceptor2))]
        public virtual string VirtualMethod(int id) => id.ToString();
    }

    public interface IFooBar
    {
        void Method1();
        void Method2();
        void Method3();
    }

    [Interceptor(typeof(FooBarInterceptor1))]
    public class FooBar : IFooBar
    {
        public void Method1() { }
        [NonInterceptable]
        public void Method2() { }
        public void Method3() { }
    }
    class BarInterceptor1
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();
            var returnValue=context.GetReturnValue<string>();
            int num = int.Parse(returnValue);
            context.SetReturnValue((num+1).ToString());
        }
    }
    class BarInterceptor2
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            var argumentVal= context.GetArgument<int>("id");
            context.SetArgument("id",argumentVal+1);
            await context.ProceedAsync();
        }
    }

    class FooBarInterceptor1
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();
            Compose.num++;
        }
    }

    
}
