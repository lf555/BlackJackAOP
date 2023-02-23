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
        where T : notnull
    {
        public virtual T M1(T t) => t;
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
}
