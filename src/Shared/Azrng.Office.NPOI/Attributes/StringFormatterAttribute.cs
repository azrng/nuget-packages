namespace Azrng.Office.NPOI.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class StringFormatterAttribute : Attribute
    {
        public StringFormatterAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; set; }
    }
}