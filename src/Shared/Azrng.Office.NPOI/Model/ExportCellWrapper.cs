using Azrng.Office.NPOI.Styles;

namespace Azrng.Office.NPOI.Model
{
    public class ExportCellWrapper
    {
        public ExportCellWrapper(int columnIndex, string value)
        {
            ColumnIndex = columnIndex;
            Value = value;
        }

        public ExportCellWrapper(int columnIndex, bool isPrimaryKeyColumn, string value)
        {
            ColumnIndex = columnIndex;
            IsPrimaryKeyColumn = isPrimaryKeyColumn;
            Value = value;
        }

        public ExportCellWrapper(int columnIndex, bool isPrimaryKeyColumn, string value, BaseStyle? cellStyle)
        {
            ColumnIndex = columnIndex;
            IsPrimaryKeyColumn = isPrimaryKeyColumn;
            Value = value;
            CellStyle = cellStyle;
        }

        /// <summary>
        /// 单元格列索引
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// 单元格内容
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 是否是主键列
        /// </summary>
        public bool IsPrimaryKeyColumn { get; set; }

        /// <summary>
        /// 列合并,相邻的列都为true则合并
        /// </summary>
        public bool MergeColumn { get; set; }

        /// <summary>
        /// 根据主键合并行
        /// </summary>
        public bool MergedRowByPrimaryKey { get; set; }

        /// <summary>
        /// 不根据主键合并行
        /// </summary>
        public bool MergedRowAlone { get; set; }

        /// <summary>
        /// 单元格样式
        /// </summary>
        public BaseStyle? CellStyle { get; set; }
    }
}