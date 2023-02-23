namespace BlackJackAOP
{
    public static class MethodInvokerBuilder
    {
        public static IMethodInvokerBuilder Instance { get; internal set; } = default!;
    }
}
