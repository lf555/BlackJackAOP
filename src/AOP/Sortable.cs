namespace BlackJackAOP
{
    public sealed class Sortable<T>
    {
        public int Order { get; }

        public T Value { get; set; }

        public Sortable(int order, T value)
        {
            Order = order;
            Value = value;
        }
    }
}
