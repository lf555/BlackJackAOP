using BlackJackAOP;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class Property
    {
        [Fact]
        public void T1()
        {
            var service = new ServiceCollection()
                .AddSingleton<FooBaz>()
                .AddSingleton<Service>()
                .BuildInterceptableServiceProvider(interception => {
                    interception.RegisterInterceptors(RegisterInterceptors1);
                })
                .GetRequiredService<FooBaz>();

            service.p4 = 0;
            Assert.True(service.P1==1&&service.P2==1&&service.P3==1&&service.p4==1);
        }

        static void RegisterInterceptors1(IInterceptorRegistry registry)
        {
            registry.For<PropertyInterceptor1>("a").ToGetMethod<FooBaz>(1, it => it.P2).ToProperty<FooBaz>(1, it => it.P3);
            registry.For<PropertyInterceptor2>("b").ToSetMethod<FooBaz>(1, it => it.p4);
        }

        [Fact]
        public void T2()
        {
            var service = new ServiceCollection()
                .AddSingleton<FooQux>()
                .AddSingleton<Service>()
                .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors2))
                .GetRequiredService<FooQux>();

            Assert.True(service.P1==1&&service.P2==1&&service.M1()==1);
        }

        static void RegisterInterceptors2(IInterceptorRegistry registry)
        {
            registry.For<PropertyInterceptor1>("").ToAllMethods<FooQux>(1);
        }

        [Fact]
        public void T3()
        {
            var service = new ServiceCollection()
                .AddSingleton<FooQux>()
                .AddSingleton<Service>()
                .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors3))
                .GetRequiredService<FooQux>();

            Assert.True(service.P1 == 1 && service.P2 == 1 && service.M1() == 1);
        }

        static void RegisterInterceptors3(IInterceptorRegistry registry)
        {
            var methods = typeof(FooQux).GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public).Where(it=>it.DeclaringType!=typeof(object)).ToArray();
            registry.For<PropertyInterceptor1>("").ToMethods<FooQux>(1,methods);
        }
    }

    public class FooBaz
    {
        [Interceptor(typeof(PropertyInterceptor1),"a")]
        public virtual int P1 { get; set; }
        public virtual int P2 { get; set; }
        public virtual int P3 { get; set; }
        public virtual int p4 { get; set; }
    }

    public class FooQux
    {
        public virtual int P1 { get; set; }
        public virtual int P2 { get; set; }
        public virtual int M1() => 0;
    }
    public class Service { }

    class PropertyInterceptor1
    {
        public PropertyInterceptor1(string name,Service service)
        {
            Assert.NotNull(name);
            Assert.NotNull(service);
        }
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            context.SetReturnValue(1);
            await ValueTask.CompletedTask;
        }
    }
    class PropertyInterceptor2
    {
        public PropertyInterceptor2(string name)
        {

        }
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            var value = context.GetReturnValue<int>();
            context.SetArgument(0, 1);
            await context.ProceedAsync();
        }
    }
}
