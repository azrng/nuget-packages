namespace CustomExcel.Exporter.Exporters;

/// <summary>
/// 基础sheet导出配置
/// </summary>
public class BaseSheetExportConfig
{
    /// <summary>
    /// 工作簿
    /// </summary>
    /// <param name="sheetName">名称</param>
    /// <param name="displayGridlines">是否显示网格线</param>
    public BaseSheetExportConfig(string sheetName, bool displayGridlines = true)
    {
        SheetName = sheetName;
        DisplayGridlines = displayGridlines;
    }

    /// <summary>
    /// sheet名称
    /// </summary>
    public string SheetName { get; set; }

    /// <summary>
    /// 是否显示网格线
    /// </summary>
    public bool DisplayGridlines { get; set; }

    /// <summary>
    /// 默认行高
    /// </summary>
    public int DefaultRowHeight { get; set; }

    /// <summary>
    /// 默认列宽
    /// </summary>
    public int DefaultColumnWidth { get; set; }
}