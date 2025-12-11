using Azrng.Office.NPOI.Styles;

namespace Azrng.Office.NPOI.Model
{
    /// <summary>
    /// 导出的第一行标题
    /// </summary>
    public class ExportSheetTitle
    {
        public ExportSheetTitle(string title, int rowHeight = 30, TitleCellStyle? style = null)
        {
            Title = title;
            RowHeight = rowHeight > 1 ? rowHeight : 14;
            Style = style ?? new TitleCellStyle();
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 标题样式
        /// </summary>
        public TitleCellStyle? Style { get; set; }

        /// <summary>
        /// 行高
        /// </summary>
        public int RowHeight { get; set; }
    }
}