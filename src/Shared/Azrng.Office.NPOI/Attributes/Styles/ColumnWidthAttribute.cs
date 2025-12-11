namespace Azrng.Office.NPOI.Attributes.Styles
{
    /// <summary>
    /// 列宽
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class ColumnWidthAttribute : Attribute
    {
        public ColumnWidthAttribute(int minWidth, int maxWidth)
        {
            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }

        public int MinWidth { get; set; }

        public int MaxWidth { get; set; }
    }
}