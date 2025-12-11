namespace Azrng.Office.NPOI.Model
{
    /// <summary>
    /// 导出列包装器
    /// </summary>
    public class ExportColumnWrapper
    {
        public ExportColumnWrapper(int columnIndex, string columnName)
        {
            ColumnIndex = columnIndex;
            ColumnName = columnName;
        }

        /// <summary>
        /// 列索引
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }

        public int? MinWidth { get; set; }

        public int? MaxWidth { get; set; }
    }
}
