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
    public class Lifetime
    {
        [Fact]
        public void T1()
        {
            var service = new ServiceCollection()
                .AddSingleton<L1>()
                .AddScoped<Model>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<L1>();

            service.M1();
        }

        [Fact]
        public void T2()
        {
            var service = new ServiceCollection()
                .AddSingleton<L2>()
                .AddTransient<Model>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<L2>();

            service.M1();
        }
    }

    public class L1
    {
        [DI]
        public virtual void M1(Model? model1=null,Model? model2=null)
        {
            Assert.True(model1 is not null&&model1==model2);
             
        }
    }

    public class L2
    {
        [DI]
        public virtual void M1(Model? model1 = null, Model? model2 = null)
        {
            Assert.True(model1 != model2);

        }
    }

}
