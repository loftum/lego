namespace Lego.Car
{
    public struct Sampled<T> where T : struct
    {
        private T _value;
        public T LastValue { get; private set; }

        public T Value
        {
            get => _value;
            set
            {
                LastValue = _value;
                _value = value;
            }
        }

        public bool HasChanged => !LastValue.Equals(Value);
    }
}