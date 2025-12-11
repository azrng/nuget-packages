using Azrng.Core.Extension;
using Azrng.Office.NPOI.Attributes.Styles;
using Azrng.Office.NPOI.Styles;
using System.Reflection;

namespace Azrng.Office.NPOI.Attributes
{
    /// <summary>
    /// 列属性
    /// </summary>
    public class ColumnProperty
    {
        public ColumnProperty(PropertyInfo property,int columnIndex)
        {
            ColumnIndex = columnIndex;
            Name = property.GetCustomAttribute(typeof(ColumnNameAttribute)) is ColumnNameAttribute nameAttr
                ? nameAttr.Name
                : property.Name;

            if (property.GetCustomAttribute(typeof(ColumnWidthAttribute)) is ColumnWidthAttribute widthAttr)
            {
                MinWidth = widthAttr.MinWidth;
                MaxWidth = widthAttr.MaxWidth;
            }

            if (property.GetCustomAttribute(typeof(HeaderStyleAttribute)) is HeaderStyleAttribute headerAttr)
                HeaderStyle = headerAttr.Style;

            if (property.GetCustomAttribute(typeof(ColumnStyleAttribute)) is ColumnStyleAttribute columnAttr)
                ColumnStyle = columnAttr.Style;

            if (property.GetCustomAttribute(typeof(StringFormatterAttribute)) is StringFormatterAttribute formatAttr)
                StringFormat = formatAttr.Format;

            if (property.GetCustomAttribute(typeof(MergeRowAttribute)) is MergeRowAttribute)
                MergedRowByPrimaryKey = true;

            if (property.GetCustomAttribute(typeof(MergeRowAloneAttribute)) is MergeRowAloneAttribute)
            {
                MergedRowAlone = true;
                MergedRowByPrimaryKey = false;
            }

            if (property.GetCustomAttribute(typeof(PrimaryKeyAttribute)) is PrimaryKeyAttribute)
                IsPrimaryColumn = true;

            if (property.GetCustomAttribute(typeof(RowHeightAttribute)) is RowHeightAttribute rowHeightAttr)
                HeaderRowHeight = rowHeightAttr.RowHeight;

            PropertyInfo = property;
        }

        /// <summary>
        /// 单元格最小宽度
        /// </summary>
        public int? MinWidth { get; set; }

        /// <summary>
        /// 单元格最大宽度
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// 属性所在的列索引
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 表头样式
        /// </summary>
        public BaseStyle? HeaderStyle { get; set; }

        /// <summary>
        /// 整列样式
        /// </summary>
        public BaseStyle? ColumnStyle { get; set; }

        /// <summary>
        /// 格式化内容
        /// </summary>
        public string? StringFormat { get; set; }

        /// <summary>
        /// 参与合并行
        /// </summary>
        public bool MergedRowByPrimaryKey { get; set; }

        /// <summary>
        /// 单独合并行,优先级大于Rowmerged,与主键无关,只与当前值有关
        /// </summary>
        public bool MergedRowAlone { get; set; }

        /// <summary>
        /// 是否是主键列
        /// </summary>
        public bool IsPrimaryColumn { get; set; }

        /// <summary>
        /// 行高
        /// </summary>
        public int HeaderRowHeight { get; set; }

        /// <summary>
        /// 列对应的对象属性
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 获取该属性在cell中展示的文本
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public string GetCellValue(object target)
        {
            var objectValue = PropertyInfo.GetValue(target);

            var cellValue = string.Empty;
            if (objectValue != null)
            {
                if (PropertyInfo.PropertyType == typeof(DateTime) || PropertyInfo.PropertyType == typeof(DateTime?))
                {
                    var format = StringFormat.IsNullOrWhiteSpace() ? ExportConfig.DefaultDateFormat : StringFormat;
                    cellValue = ((DateTime)objectValue).ToString(format);
                }
                else
                {
                    cellValue = objectValue.ToString();
                }
            }

            return cellValue;
        }
    }
}