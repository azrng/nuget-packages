namespace Azrng.Office.NPOI.Model
{
    public class ExportSheetWrapper
    {
        /// <summary>
        /// 工作簿包装
        /// </summary>
        /// <param name="columns">列</param>
        /// <param name="rows">行</param>
        public ExportSheetWrapper(List<ExportColumnWrapper> columns, List<ExportRowWrapper> rows)
        {
            Columns = columns;
            Rows = rows;
        }

        public List<ExportColumnWrapper> Columns { get; set; }

        public List<ExportRowWrapper> Rows { get; set; }
    }
}
