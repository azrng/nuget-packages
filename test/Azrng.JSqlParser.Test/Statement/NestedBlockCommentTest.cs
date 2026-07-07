using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 嵌套块注释测试，对齐上游 JSqlParser NestedCommentTest。
///
/// 背景：T078 修复前词法规则 BLOCK_COMMENT: '/*' .*? '*/' 是非贪婪匹配，
/// 遇到首个 */ 即结束，导致嵌套注释（如 /* 外 /* 内 */ 外 */）剩余文本抛 JSqlParserException。
/// 修复后用 lexer 模式 IN_BLOCK_COMMENT + commentNesting 深度计数器正确处理任意深度嵌套。
///
/// 与上游一致的行为约定：
///   - 注释只要求"解析不抛错"，round-trip 时注释被丢弃（上游 toString 也不保留注释）
///   - 未闭合的 /* 必须抛错（上游同样拒绝）
///   - /*+ ... */ Oracle Hint 由 ORACLE_HINT_ML 规则保留，不走嵌套注释路径
/// </summary>
public class NestedBlockCommentTest
{
    private static void AssertParses(string sql)
    {
        // 对齐上游 assertDoesNotThrow：仅保证解析不抛错
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
    }

    private static void AssertThrows(string sql)
    {
        Assert.ThrowsAny<JSqlParserException>(() => CCJSqlParserUtil.Parse(sql));
    }

    #region 基础嵌套（对齐上游 testFlatBlockComment / testNestedBlockComment / testDeeplyNestedBlockComment）

    [Fact]
    public void FlatBlockComment_ShouldParse()
    {
        // 非嵌套块注释仍正常（回归保护）
        AssertParses("SELECT /* simple comment */ 1");
    }

    [Fact]
    public void NestedBlockComment_OneLevel_ShouldParse()
    {
        // 一层嵌套：核心修复目标
        AssertParses("SELECT /* outer /* inner */ outer */ 1");
    }

    [Fact]
    public void NestedBlockComment_TwoLevels_ShouldParse()
    {
        // 两层嵌套
        AssertParses("SELECT /* level 0 /* level 1 /* level 2 */ back to 1 */ back to 0 */ 1");
    }

    [Fact]
    public void NestedBlockComment_InWhereClause_ShouldParse()
    {
        AssertParses("SELECT * FROM t WHERE /* a /* nested */ comment */ x = 1");
    }

    #endregion

    #region 边界字符（对齐上游 testNestedCommentContainingStars / Slashes）

    [Fact]
    public void NestedComment_ContainingStars_ShouldParse()
    {
        // 注释内含连续星号
        AssertParses("SELECT /* ** /* * */ ** */ 1");
    }

    [Fact]
    public void NestedComment_ContainingSlashes_ShouldParse()
    {
        // 注释内含 // 与 --（这些在块注释里不是注释终结符）
        AssertParses("SELECT /* // /* -- */ // */ 1");
    }

    [Fact]
    public void EmptyNestedComment_ShouldParse()
    {
        AssertParses("SELECT /* /**/ */ 1");
    }

    [Fact]
    public void MultipleNestedCommentsInSequence_ShouldParse()
    {
        AssertParses("SELECT /* /* a */ */ 1, /* /* b */ */ 2");
    }

    #endregion

    #region 行注释与块注释交互（对齐上游 testLineComment* 系列）

    [Fact]
    public void LineComment_AfterValue_ShouldParse()
    {
        // 行注释仍正常（回归保护，T077 已覆盖，这里复测）
        AssertParses("SELECT 1 -- line comment");
    }

    [Fact]
    public void LineComment_InsideBlockComment_ShouldParse()
    {
        // 块注释内的 -- 不应触发行注释逻辑
        AssertParses("SELECT /* -- not a line comment */ 1");
    }

    [Fact]
    public void MultilineNestedComment_ShouldParse()
    {
        AssertParses(
            "SELECT *\n" +
            "/*\n" +
            "  /*\n" +
            "    nested across lines\n" +
            "  */\n" +
            "*/\n" +
            "FROM t");
    }

    #endregion

    #region Oracle Hint（对齐上游 testOracleHintPreserved）

    [Fact]
    public void OracleHint_ShouldBePreserved()
    {
        // /*+ ... */ 是 Oracle Hint，由 ORACLE_HINT_ML 规则保留，不参与嵌套计数
        var stmt = CCJSqlParserUtil.Parse("SELECT /*+ FULL(t) */ * FROM t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT /*+ FULL(t) */ * FROM t", stmt!.ToString());
    }

    #endregion

    #region 未闭合注释必须抛错（对齐上游语义）

    [Fact]
    public void UnterminatedBlockComment_ShouldThrow()
    {
        // 未闭合的 /* 应被拒绝（修复后保持此行为，不能因嵌套逻辑放宽）
        AssertThrows("SELECT /* unterminated 1");
    }

    [Fact]
    public void UnterminatedNestedBlockComment_ShouldThrow()
    {
        // 嵌套未闭合：开 2 层只闭 1 层，仍应抛错
        AssertThrows("SELECT /* outer /* inner */ no-outer-close 1");
    }

    #endregion

    #region round-trip 行为固化（注释被丢弃，与上游一致）

    [Fact]
    public void NestedComment_RoundTrip_ShouldDropComment()
    {
        // round-trip 时注释被丢弃（上游 toString 也不保留注释）
        var stmt = CCJSqlParserUtil.Parse("SELECT /* outer /* inner */ outer */ 1");

        Assert.Equal("SELECT 1", stmt!.ToString());
    }

    #endregion
}
