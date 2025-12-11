using Azrng.Office.NPOI.Attributes;
using Azrng.Office.NPOI.Styles;

namespace Azrng.Office.NPOI.Model
{
    public class ExportRowWrapper
    {
        public ExportRowWrapper(object data, int rowIndex, List<ColumnProperty> columnProperties)
        {
            RowIndex = rowIndex;

            //动态设置的单元格样式
            var dynamicStylePair = new Dictionary<string, BaseStyle>();
            if (data is DynamicCellStyle)
                dynamicStylePair = ((DynamicCellStyle)data).PropertyNameStylePair.ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in columnProperties)
            {
                var cell = new ExportCellWrapper(item.ColumnIndex, item.IsPrimaryColumn, item.GetCellValue(data))
                           {
                               MergedRowAlone = item.MergedRowAlone, MergedRowByPrimaryKey = item.MergedRowByPrimaryKey
                           };

                //动态样式优先
                if (dynamicStylePair.TryGetValue(item.PropertyInfo.Name, out var style))
                    cell.CellStyle = style;
                else
                    cell.CellStyle = item.ColumnStyle;
                Cells.Add(cell);
            }

            PrimaryKey = string.Join("_", Cells.Where(x => x.IsPrimaryKeyColumn).Select(x => x.Value));
        }

        public ExportRowWrapper(List<ExportCellWrapper> cells, int rowIndex, int rowHeight = 0)
        {
            Cells = cells;
            RowIndex = rowIndex;
            RowHeight = rowHeight;
        }

        /// <summary>
        /// 数据所在的excel行索引
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        /// 行高
        /// </summary>
        public int RowHeight { get; set; }

        /// <summary>
        /// 行所包含的单元格
        /// </summary>
        public List<ExportCellWrapper> Cells { get; set; } = new List<ExportCellWrapper>();

        /// <summary>
        /// 唯一键
        /// </summary>
        public string? PrimaryKey { get; }
    }
}