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
    public class Expression
    {
        public static List<int> list = new List<int>();
        [Fact]
        public void T1()
        {
            var service = new ServiceCollection()
                .AddSingleton<IQux, Qux>()
                .BuildInterceptableServiceProvider(interception=> interception.RegisterInterceptors(RegisterInterceptors))
                .GetRequiredService<IQux>();

            var result = service.Add(0);
            Assert.True(result == 1 && list.Count == 2 && list[0] == 2 && list[1] == 3);
        }
        static void RegisterInterceptors(IInterceptorRegistry registry)
        {
            registry.For<QuxInterceptor2>().ToMethod<Qux>(1, it => it.Add(default));
            registry.For<QuxInterceptor1>().ToMethod<Qux>(2, it => it.Add(default));
        }

        [Fact]
        public void T2()
        {
            var service = new ServiceCollection()
                .AddSingleton(typeof(C2<>))
                .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors1))
                .GetRequiredService<C2<int>>();

            service.M1(2);
        }

        static void RegisterInterceptors1(IInterceptorRegistry registry)
        {
            registry.For<C1Intercetptor>().ToMethod<C2<int>>(1, it => it.M1(default));
        }

    }

    public interface IQux
    {
        int Add(int start);
    }

    public class Qux : IQux
    {
        public int Add(int start) => start + 1;
    }

    class QuxInterceptor1
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();
            var result = context.GetReturnValue<int>();
            Expression.list.Add(result + 1);
        }
    }

    class QuxInterceptor2
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();
            var result = context.GetReturnValue<int>();
            Expression.list.Add(result + 2);
        }
    }
}
