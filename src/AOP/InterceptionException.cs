namespace BlackJackAOP
{
    [Serializable]
    public class InterceptionException : Exception
    {
        public InterceptionException() { }

        public InterceptionException(string message) : base(message) { }

        public InterceptionException(string message, Exception inner) : base(message, inner) { }

        protected InterceptionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}