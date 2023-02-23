namespace BlackJackAOP
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class InterceptorAttribute : Attribute
    {
        public Type Interceptor { get; }

        public object[] Arguments { get; }

        public int Order { get; set; }

        public InterceptorAttribute(params object[] arguments) : this(null, arguments) { }

        public InterceptorAttribute(Type? interceptor, params object[] arguments)
        {
            Interceptor = interceptor ?? GetType();
            Arguments = arguments;
        }
    }
}
