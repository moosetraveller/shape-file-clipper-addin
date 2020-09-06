

namespace Geomo.Util
{
    class ComboBoxValue<T>
    {
        public string Label { get; set; }

        public T Value { get; set; }

        public ComboBoxValue(T value, string label)
        {
            this.Value = value;
            this.Label = label;
        }

        public override string ToString()
        {
            return this.Label;
        }
    }
}
