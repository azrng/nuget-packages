using Azrng.Office.NPOI.Styles;
using NPOI.SS.UserModel;

namespace Azrng.Office.NPOI.Attributes.Styles
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class StyleAttribute : Attribute
    {
        public StyleAttribute(bool isBold = false,
                              bool wrapText = false,
                              short fontColor = 8,
                              int fontSize = 11,
                              string fontName = "宋体",
                              short fillForegroundColor = -1,
                              HorizontalAlignment horizontalAlign = HorizontalAlignment.Left,
                              VerticalAlignment verticalAlign = VerticalAlignment.Center, BorderStyle borderBottom = BorderStyle.None,
                              BorderStyle borderLeft = BorderStyle.None,
                              BorderStyle borderRight = BorderStyle.None, BorderStyle borderTop = BorderStyle.None,
                              bool showAllBorder = false, short bottomBorderColor = 8, short leftBorderColor = 8,
                              short rightBorderColor = 8, short topBorderColor = 8)
        {
            Style.IsBold = isBold;
            Style.WrapText = wrapText;
            Style.FontColor = fontColor;
            Style.FontSize = fontSize;
            Style.FontName = fontName;
            if (fillForegroundColor > 0)
                Style.FillForegroundColor = fillForegroundColor;
            Style.HorizontalAlign = horizontalAlign;
            Style.VerticalAlign = verticalAlign;

            Style.BorderBottom = borderBottom;
            Style.BorderLeft = borderLeft;
            Style.BorderRight = borderRight;
            Style.BorderTop = borderTop;
            Style.BottomBorderColor = bottomBorderColor;
            Style.LeftBorderColor = leftBorderColor;
            Style.RightBorderColor = rightBorderColor;
            Style.TopBorderColor = topBorderColor;

            if (showAllBorder)
            {
                Style.BorderBottom = BorderStyle.Thin;
                Style.BorderLeft = BorderStyle.Thin;
                Style.BorderRight = BorderStyle.Thin;
                Style.BorderTop = BorderStyle.Thin;
            }
        }

        public BaseStyle Style { get; set; } = new BaseStyle();
    }
}