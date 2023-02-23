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
    public class IOC
    {
        [Fact]
        public void T1()
        {
            var service = new ServiceCollection()
                .AddSingleton<DI>()
                .AddSingleton<Model>()
                .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors))
                .GetRequiredService<DI>();

            service.M1(2);
            service.M2();
            Assert.NotNull(service.P1);
            Assert.NotNull(service.P2);
            Assert.NotNull(service.p3);
            service.p4 = null;
            Assert.NotNull(service.p4);
        }

        static void RegisterInterceptors(IInterceptorRegistry registry)
        {
            registry.For<DIAttribute>()
                .ToMethod<DI>(1, it => it.M2(default!))
                .ToProperty<DI>(1,it=>it.P2)
                .ToGetMethod<DI>(1,it=>it.p3)
                .ToSetMethod<DI>(1,it=>it.p4);
        }
    }

    public class DI
    {
        [DI]
        public virtual Model? P1 { get; set; }
        public virtual Model? P2 { get; set; }
        public virtual Model? p3 { get; set; }
        public virtual Model? p4 { get; set; }
        [DI]
        public virtual void M1(int i,Model? model = null)
        {
            Assert.NotNull(model);
        }

        public virtual void M2(Model? model = null)
        {
            Assert.NotNull(model);
        }
    }
    public class Model { }
}
