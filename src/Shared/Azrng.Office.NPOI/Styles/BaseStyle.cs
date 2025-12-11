using NPOI.SS.UserModel;
using System.Text;

namespace Azrng.Office.NPOI.Styles
{
    public class BaseStyle
    {
        public BaseStyle() { }

        public BaseStyle(bool isBold, bool wrapText, short fontColor, int fontSize, string fontName,
                         short? fillForegroundColor = null, HorizontalAlignment horizontalAlign = HorizontalAlignment.Left,
                         VerticalAlignment verticalAlign = VerticalAlignment.Center, BorderStyle borderBottom = BorderStyle.None,
                         BorderStyle borderLeft = BorderStyle.None,
                         BorderStyle borderRight = BorderStyle.None, BorderStyle borderTop = BorderStyle.None, bool showAllBorder = false,
                         short bottomBorderColor = 8, short leftBorderColor = 8,
                         short rightBorderColor = 8, short topBorderColor = 8)
        {
            IsBold = isBold;
            WrapText = wrapText;
            FontColor = fontColor;
            FontSize = fontSize;
            FontName = fontName;
            FillForegroundColor = fillForegroundColor;
            HorizontalAlign = horizontalAlign;
            VerticalAlign = verticalAlign;

            BorderBottom = borderBottom;
            BorderLeft = borderLeft;
            BorderRight = borderRight;
            BorderTop = borderTop;
            BottomBorderColor = bottomBorderColor;
            LeftBorderColor = leftBorderColor;
            RightBorderColor = rightBorderColor;
            TopBorderColor = topBorderColor;

            if (showAllBorder)
            {
                BorderBottom = BorderStyle.Thin;
                BorderLeft = BorderStyle.Thin;
                BorderRight = BorderStyle.Thin;
                BorderTop = BorderStyle.Thin;
            }
        }

        /// <summary>
        /// 加粗
        /// </summary>
        public bool IsBold { get; set; }

        /// <summary>
        /// 自动换行
        /// </summary>
        public bool WrapText { get; set; }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public short FontColor { get; set; } = 8;

        /// <summary>
        /// 字体大小
        /// </summary>
        public int FontSize { get; set; } = 11;

        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName { get; set; } = "宋体";

        /// <summary>
        /// 背景色
        /// </summary>
        public short? FillForegroundColor { get; set; }

        /// <summary>
        /// 水平对齐
        /// </summary>
        public HorizontalAlignment HorizontalAlign { get; set; } = HorizontalAlignment.Left;

        /// <summary>
        /// 垂直对齐
        /// </summary>
        public VerticalAlignment VerticalAlign { get; set; } = VerticalAlignment.Center;

        public BorderStyle BorderBottom { get; set; }

        public BorderStyle BorderLeft { get; set; }

        public BorderStyle BorderRight { get; set; }

        public BorderStyle BorderTop { get; set; }

        public short BottomBorderColor { get; set; }

        public short LeftBorderColor { get; set; }

        public short RightBorderColor { get; set; }

        public short TopBorderColor { get; set; }

        public override string ToString()
        {
            var str = new StringBuilder();
            foreach (var item in GetType().GetProperties())
            {
                str.Append(item.Name).Append('_').Append(item.GetValue(this)?.ToString() ?? string.Empty).Append("__");
            }

            return str.ToString();
        }
    }

    ///// <summary>
    ///// 水平对齐
    ///// </summary>
    //public enum HorizontalAlign
    //{
    //    /// <summary>
    //    /// 左侧
    //    /// </summary>
    //    Left = 1,

    //    /// <summary>
    //    /// 居中
    //    /// </summary>
    //    Center = 2,

    //    /// <summary>
    //    /// 右侧
    //    /// </summary>
    //    Right = 3,
    //}

    ///// <summary>
    ///// 垂直对齐
    ///// </summary>
    //public enum VerticalAlignment
    //{
    //    /// <summary>
    //    /// 头部
    //    /// </summary>
    //    Top = 0,

    //    /// <summary>
    //    /// 居中
    //    /// </summary>
    //    Center = 1,

    //    /// <summary>
    //    /// 下面
    //    /// </summary>
    //    Bottom = 2,
    //}
}