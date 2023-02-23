using BlackJackAOP;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class Simple
    {
        public static object? obj1 = null;
        public static object? obj2 = null;
        public static object? obj3 = null;
        public static object? obj4 = null;
        [Fact]
        public void T1()
        {
           
            var service= new ServiceCollection()
                .AddSingleton<Foo>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Foo>();

            service.VirtualMethod();
            Assert.NotNull(obj1);
        }

        [Fact]
        public void T2()
        {
            var service = new ServiceCollection()
                .AddSingleton<IFoo,Foo>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoo>();

            service.InterfaceMethod();
            Assert.NotNull(obj2);
        }

        [Fact]
        public void T3()
        {
            var provider = new ServiceCollection()
                .AddSingleton<Foo>()
                .AddSingleton<IFoo, Foo>()
                .BuildInterceptableServiceProvider();
            var @interface = provider.GetRequiredService<IFoo>();
            var service = provider.GetRequiredService<Foo>();

            @interface.InterfaceMethod();
            service.VirtualMethod();
            Assert.True(obj3 is not null&&obj4 is not null);
        }
        
    }

    class FooInterceptor1
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            Simple.obj1 = new object();
            Simple.obj3 = new object();
            await ValueTask.CompletedTask;
        }
    }

    public interface IFoo
    {
        void InterfaceMethod();
    }

    public class Foo : IFoo
    {
        [Interceptor(typeof(FooInterceptor2))]
        public virtual void InterfaceMethod() { }

        [Interceptor(typeof(FooInterceptor1))]
        public virtual void VirtualMethod() { }
    }

    class FooInterceptor2
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            Simple.obj2 = new object();
            Simple.obj4 = new object();
            await ValueTask.CompletedTask;
        }
    }
}