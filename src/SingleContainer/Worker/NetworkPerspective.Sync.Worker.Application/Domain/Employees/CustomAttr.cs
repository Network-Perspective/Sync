namespace NetworkPerspective.Sync.Worker.Application.Domain.Employees
{
    public class CustomAttr
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool IsMultiValue { get; set; }

        public static CustomAttr Create(string name, object value)
            => new CustomAttr(name, value, false);

        public static CustomAttr CreateMultiValue(string name, object value)
            => new CustomAttr(name, value, true);

        private CustomAttr(string name, object value, bool isMultiValue)
        {
            Name = name;
            Value = value;
            IsMultiValue = isMultiValue;
        }
    }
}