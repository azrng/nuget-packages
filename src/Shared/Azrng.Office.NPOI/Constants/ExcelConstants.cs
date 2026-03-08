using System.Globalization;

namespace Azrng.Office.NPOI;

/// <summary>
/// Excel 操作相关常量
/// </summary>
public static class ExcelConstants
{
    /// <summary>
    /// Excel 行高乘数（用于将逻辑行高转换为 Excel 内部行高）
    /// </summary>
    public const int RowHeightMultiplier = 20;

    /// <summary>
    /// Excel 工作表名称最大长度
    /// </summary>
    public const int MaxSheetNameLength = 31;

    /// <summary>
    /// 默认批处理大小
    /// </summary>
    public const int DefaultBatchSize = 1000;

    /// <summary>
    /// 默认字体大小
    /// </summary>
    public const int DefaultFontSize = 11;

    /// <summary>
    /// Excel 工作表名称非法字符
    /// </summary>
    public static readonly char[] InvalidSheetNameChars = { '\\', '/', '?', '*', ':', '[', ']' };
}

/// <summary>
/// Excel 文化设置
/// </summary>
public static class ExcelCulture
{
    /// <summary>
    /// Excel 不变文化（用于格式化和解析）
    /// </summary>
    public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
}
