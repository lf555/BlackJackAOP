using BlackJackAOP;

namespace WebApp
{
    public class ServiceInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            var value= context.GetArgument<string>("msg");
            context.SetArgument("msg",value+" world");
            await context.ProceedAsync();
            var returnValue = context.GetReturnValue<string>();
            context.SetReturnValue(returnValue+" hahahahahahaha");
        }
    }
}
