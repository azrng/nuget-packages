using NPOI.SS.UserModel;

namespace Azrng.Office.NPOI.Styles
{
    /// <summary>
    /// 单元格样式
    /// </summary>
    public class TitleCellStyle : BaseStyle
    {
        /// <summary>
        /// 单元格样式
        /// </summary>
        /// <param name="isBold">是否粗体</param>
        /// <param name="wrapText">是否换行</param>
        /// <param name="fillForegroundColor">填充背景色</param>
        /// <param name="fontName">字体</param>
        /// <param name="fontSize">文字大小</param>
        /// <param name="horizontal">水平对齐</param>
        /// <param name="vertical">垂直对齐</param>
        /// <param name="borderBottom">下边框</param>
        /// <param name="borderLeft">左边框</param>
        /// <param name="borderRight">右边框</param>
        /// <param name="borderTop">头部边框</param>
        /// <param name="showAllBorder">是否显示所有边框</param>
        /// <param name="bottomBorderColor">下边框颜色</param>
        /// <param name="leftBorderColor">左边框颜色</param>
        /// <param name="rightBorderColor">右边框颜色</param>
        /// <param name="topBorderColor">头部边框颜色</param>
        /// <param name="fontColor"></param>
        public TitleCellStyle(bool isBold = true, bool wrapText = false, short fontColor = 8, short? fillForegroundColor = null,
                              string fontName = "仿宋", int fontSize = 18, HorizontalAlignment horizontal = HorizontalAlignment.Center,
                              VerticalAlignment vertical = VerticalAlignment.Center, BorderStyle borderBottom = default,
                              BorderStyle borderLeft = BorderStyle.None,
                              BorderStyle borderRight = BorderStyle.None, BorderStyle borderTop = BorderStyle.None,
                              bool showAllBorder = false, short bottomBorderColor = 8, short leftBorderColor = 8,
                              short rightBorderColor = 8, short topBorderColor = 8) : base(isBold, wrapText, fontColor, fontSize,
            fontName, fillForegroundColor, horizontal, vertical, borderBottom,
            borderLeft,
            borderRight, borderTop, showAllBorder, bottomBorderColor, leftBorderColor,
            rightBorderColor, topBorderColor) { }
    }
}