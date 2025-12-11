using System.Collections.Concurrent;

namespace Azrng.Office.NPOI.Styles
{
    public class DynamicCellStyle
    {
        public ConcurrentDictionary<string, BaseStyle> PropertyNameStylePair { get; set; } = new ConcurrentDictionary<string, BaseStyle>();

        /// <summary>
        /// 动态设置单元格样式
        /// 样式将完全覆盖默认的column样式
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="style"></param>
        public void SetCellStyle(string propertyName, BaseStyle style)
        {
            PropertyNameStylePair.AddOrUpdate(propertyName, style, (x, y) => style);
        }
    }
}