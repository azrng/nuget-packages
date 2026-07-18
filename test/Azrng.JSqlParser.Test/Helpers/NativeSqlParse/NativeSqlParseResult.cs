namespace Azrng.JSqlParser.Test.Helpers.NativeSqlParse;

/// <summary>从 SynyiTools.Service.NativeSqlParserService 拷贝的结果 DTO。</summary>
public sealed class NativeSqlParseResult
{
    public IDictionary<string, string> TableNameList { get; set; } = new Dictionary<string, string>();

    public List<NativeSqlSelectColumnInfo> ColumnList { get; set; } = [];

    public List<NativeSqlOperatorInfo> WhereList { get; set; } = [];
}
