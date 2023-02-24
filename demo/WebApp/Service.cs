using BlackJackAOP;

namespace WebApp
{
    public interface IService
    {
        string Method(string msg);
    }
    public class Service : IService
    {
        [Interceptor(typeof(ServiceInterceptor))]
        public string Method(string msg)
        {
            return msg;
        }
    }
}
