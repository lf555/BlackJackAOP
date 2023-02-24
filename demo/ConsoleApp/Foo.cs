using BlackJackAOP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public interface IFoo
    {
        int PrintReturnValue();
        string SetReturnValue();
        void PrintParameters(int a,string b);
        int SetParameters(int num);
    }

    public class Foo : IFoo
    {
        [Interceptor(typeof(PrintReturnValueInterceptor))]
        public virtual int PrintReturnValue()
        {
            return 0;
        }

        [Interceptor(typeof(PrintParametersInterceptor))]
        public virtual void PrintParameters(int a,string b)
        {
        }

        [Interceptor(typeof(SetParametersInterceptor))]
        public virtual int SetParameters(int num)
        {
            return num;
        }

        [Interceptor(typeof(SetReturnValueInterceptor))]
        public virtual string SetReturnValue()
        {
            return "hello";
        }
    }
}
