namespace BlackJackAOP
{
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Method| AttributeTargets.Property| AttributeTargets.Class)]

    public class NonInterceptableAttribute: Attribute
    {
    }
}
