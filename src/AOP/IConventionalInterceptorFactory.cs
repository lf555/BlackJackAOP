namespace BlackJackAOP
{
    public interface IConventionalInterceptorFactory
    {
        InvokeDelegate CreateInterceptor(Type interceptorType, params object[] arguments);

        InvokeDelegate CreateInterceptor(object interceptor);
    }
}
