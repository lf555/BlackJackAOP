using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackAOP
{
    public class DIAttribute:InterceptorAttribute
    {
        public DIAttribute() => Order = int.MinValue;
        public async ValueTask InvokeAsync(InvocationContext context)
        {
            if (context.MethodInfo.IsSpecialName && context.MethodInfo.Name.StartsWith("get_"))
            {
                var result = context.InvocationServices.GetRequiredService(context.MethodInfo.ReturnType);
                context.SetReturnValue(result);
                return;
            }

            dynamic? arguments=null;
            if (context.MethodInfo.IsSpecialName && context.MethodInfo.Name.StartsWith("set_"))
            {
                arguments= context.MethodInfo.GetParameters()
                    .Select(it => new { Name = it.Name, Value = context.InvocationServices.GetRequiredService(it.ParameterType) });
            }
            else
            {
                arguments = context.MethodInfo.GetParameters()
                .Where(it => it.IsOptional)
                .Select(it => new { Name = it.Name, Value = context.InvocationServices.GetRequiredService(it.ParameterType) });
            }

            foreach(var arg in arguments)
                context.SetArgument(arg.Name!,arg.Value);

            await context.ProceedAsync();
        }
    }
}
