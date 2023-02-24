// See https://aka.ms/new-console-template for more information
using BlackJackAOP;
using ConsoleApp;
using Microsoft.Extensions.DependencyInjection;

var provider = new ServiceCollection()
                .AddSingleton<IFoo, Foo>()
                .AddSingleton<Foo>()
                .AddScoped<IBar,Bar>()
                .AddScoped<Bar>()
                .AddTransient<Baz>()
                .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(registry =>
                {
                    registry.For<BarInterceptor1>().ToMethod<Baz>(1, it => it.Method());
                    registry.For<BarInterceptor2>().ToMethod<Baz>(2, it => it.Method());
                }));

var foo = provider.GetRequiredService<IFoo>();
var bar = provider.GetRequiredService<IBar>();
var baz=provider.GetRequiredService<Baz>();

foo.PrintParameters(6,"hello world");
Console.WriteLine();

foo.PrintReturnValue();
Console.WriteLine();

var parameterValue= foo.SetParameters(1);
Console.WriteLine($"{nameof(foo.SetParameters)}改变后的值：{parameterValue}");
Console.WriteLine();

var returnValue= foo.SetReturnValue();
Console.WriteLine($"{nameof(foo.SetReturnValue)}改变后的值：{returnValue}");
Console.WriteLine();
Console.WriteLine();

bar.Method();
Console.WriteLine();
Console.WriteLine();

baz.Method();




