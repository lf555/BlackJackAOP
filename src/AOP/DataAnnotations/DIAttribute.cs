using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackAOP
{
    public class DIAttribute:InterceptorAttribute
    {
        public DIAttribute() => Order = int.MinValue;
        public virtual async ValueTask InvokeAsync(InvocationContext context)
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
                .Where(it => {
                    bool notIgnore = true;
                    if (typeof(IEnumerable).IsAssignableFrom(it.ParameterType))
                    {
                        if (it.ParameterType.IsGenericType)
                        {
                            var genericType = it.ParameterType.GetGenericArguments()[0];
                            notIgnore = NotIgnore(genericType);
                        }
                        else if (it.ParameterType.IsArray || it.ParameterType.IsSZArray)
                        {
                            var arrayType=it.ParameterType.GetElementType();
                            notIgnore = NotIgnore(arrayType!);
                        }
                        
                    }
                    return it.IsOptional && NotIgnore(it.ParameterType)&&notIgnore;
                })
                .Select(it => new { Name = it.Name, Value = context.InvocationServices.GetRequiredService(it.ParameterType) });
            }

            foreach(var arg in arguments)
                context.SetArgument(arg.Name!,arg.Value);

            await context.ProceedAsync();

            bool NotIgnore(Type type) => !type.IsPrimitive && !type.IsValueType && type != typeof(string);
        }
    }
}
