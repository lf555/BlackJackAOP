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
    public class Generic
    {
        [Fact]
        public void T1()
        {
            var service = new ServiceCollection()
                .AddSingleton(typeof(IC1<>),typeof(C1<>))
                .AddSingleton(typeof(C1<>))
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IC1<int>>();

            service.M1(2);
        }

        [Fact]
        public void T2()
        {
            var service = new ServiceCollection()
                .AddSingleton(typeof(C2<>))
                .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors))
                .GetRequiredService<C2<int>>();

            service.M1(2);
        }

        static void RegisterInterceptors(IInterceptorRegistry registry)
        {
            registry.For<C1Intercetptor>().ToMethod<C2<int>>(1, it => it.M1(default));
        }

        [Fact]
        public void T3()
        {
            var service = new ServiceCollection()
                .AddSingleton(typeof(IC3<>),typeof(C3<>))
                .AddSingleton(typeof(C3<>))
                .BuildInterceptableServiceProvider()
                .GetRequiredService<C3<int>>();

            Assert.True(true);
        }

        [Fact]
        public void T4()
        {
            var service = new ServiceCollection()
                .AddSingleton(typeof(C4<>))
                .BuildInterceptableServiceProvider()
                .GetRequiredService<C4<ServiceCollection>>();

            Assert.True(true);
        }
    }

    public interface IC1<T>
        where T:notnull
    {
        T M1(T t);
    }
    public class C1<T>:IC1<T>
        where T : notnull
    {
        [Interceptor(typeof(C1Intercetptor))]
        public virtual T M1(T t) => t;
    }

    public class C2<T>
        where T : notnull,new()
    {
        public virtual T M1(T t) => t;
    }

    public interface IC3<T>
        where T:struct
    {
        void M();
    }

    public class C3<T> : IC3<T>
        where T : struct
    {
        [Interceptor(typeof(C3Intercetptor))]
        public virtual void M() { }
    }

    public class C4<T>
        where T : class, new()
    {
        [Interceptor(typeof(C3Intercetptor))]
        public virtual void M() { }
    }

    class C1Intercetptor
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();
            var returnType = context.MethodInfo.ReturnType;
            var contextType = context.GetType();

            var getReturnValue = contextType.GetMethod("GetReturnValue")!.MakeGenericMethod(new Type[] { returnType});
            var returnValue = getReturnValue.Invoke(context,null);

            Assert.NotNull(returnValue);
        }
    }
    class C3Intercetptor
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();
        }
    }
}
