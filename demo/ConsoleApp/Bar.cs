using BlackJackAOP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public interface IBar
    {
        void Method();
    }

    [Interceptor(typeof(BarInterceptor1),Order =1)]
    public class Bar : IBar
    {
        [Interceptor(typeof(BarInterceptor1), Order = 2)]
        public virtual void Method()
        {
            Console.WriteLine("Bar.Method invoke");
        }

    }
}
