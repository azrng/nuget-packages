using NPOI.SS.UserModel;

namespace Azrng.Office.NPOI.Attributes.Styles
{
    /// <summary>
    /// 表头样式
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class HeaderStyleAttribute : StyleAttribute
    {
        public HeaderStyleAttribute(bool isBold = true, bool wrapText = false, short fontColor = 8, int fontSize = 11,
                                    string fontName = "宋体",
                                    short fillForegroundColor = -1, HorizontalAlignment horizontalAlign = HorizontalAlignment.Left,
                                    VerticalAlignment verticalAlign = VerticalAlignment.Center,
                                    BorderStyle borderBottom = BorderStyle.None, BorderStyle borderLeft = BorderStyle.None,
                                    BorderStyle borderRight = BorderStyle.None, BorderStyle borderTop = BorderStyle.None,
                                    bool showAllBorder = false, short bottomBorderColor = 8, short leftBorderColor = 8,
                                    short rightBorderColor = 8, short topBorderColor = 8)
            : base(isBold, wrapText, fontColor, fontSize, fontName,
                fillForegroundColor, horizontalAlign, verticalAlign, borderBottom, borderLeft,
                borderRight,
                borderTop, showAllBorder, bottomBorderColor, leftBorderColor, rightBorderColor,
                topBorderColor) { }
    }
}