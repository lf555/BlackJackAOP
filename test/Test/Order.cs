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
    public class Order
    {
        public static List<int> list = new List<int>();

        [Fact]
        public void T1()
        {
            var service=new ServiceCollection()
                .AddSingleton<IBaz,Baz>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IBaz>();

            var result = service.Add(0);
            Assert.True(result==1&&list.Count==2&&list[0]==2&&list[1]==3);
        }
    }

    public interface IBaz
    {
        int Add(int start);
    }

    public class Baz : IBaz
    {
        [Interceptor(typeof(BazInterceptor1),Order=2)]
        [Interceptor(typeof(BazInterceptor2), Order = 1)]
        public int Add(int start) => start + 1;
    }

    class BazInterceptor1
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();
            var result= context.GetReturnValue<int>();
            Order.list.Add(result+1);
        }
    }

    class BazInterceptor2
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            await context.ProceedAsync();
            var result = context.GetReturnValue<int>();
            Order.list.Add(result + 2);
        }
    }
}
