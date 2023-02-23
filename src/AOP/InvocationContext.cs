using System.Reflection;

namespace BlackJackAOP
{
    public abstract class InvocationContext
    {
        public object Target { get; } = default!;

        public abstract MethodInfo MethodInfo { get; }

        public abstract IServiceProvider InvocationServices { get; }

        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        public abstract TArgument GetArgument<TArgument>(string name);

        public abstract TArgument GetArgument<TArgument>(int index);

        public abstract InvocationContext SetArgument<TArgument>(string name, TArgument value);

        public abstract InvocationContext SetArgument<TArgument>(int index, TArgument value);

        public abstract TReturnValue GetReturnValue<TReturnValue>();

        public abstract InvocationContext SetReturnValue<TReturnValue>(TReturnValue value);

        internal InvokeDelegate Next { get; set; } = default!;

        protected InvocationContext(object target) => Target = target ?? throw new ArgumentNullException(nameof(target));

        public ValueTask ProceedAsync() => Next.Invoke(this);
    }
}