namespace Azrng.Office.NPOI.Attributes.Styles
{
    /// <summary>
    /// 行高(取一列中的行高最大值生效)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class RowHeightAttribute : Attribute
    {
        /// <summary>
        /// 在任意一列设置行高，取行高最大值
        /// </summary>
        /// <param name="rowHeight">行高度(不用*256)</param>
        public RowHeightAttribute(int rowHeight = 0)
        {
            RowHeight = rowHeight;
        }

        /// <summary>
        /// 行高
        /// </summary>
        public int RowHeight { get; set; }
    }
}