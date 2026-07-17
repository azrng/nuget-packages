using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Create.Function;
using Azrng.JSqlParser.Statement.Create.Procedure;
// 命名空间 Azrng.JSqlParser.Test.Statement 与 Azrng.JSqlParser.Statement 同名，
// Block/DeclareStatement/IfElseStatement 需用完全限定名避免歧义

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-12 分批 11-14 回归测试：CREATE FUNCTION/PROCEDURE + BLOCK + DECLARE + IF。
/// </summary>
public class StatementsBatch11Test
{
    #region CREATE FUNCTION / PROCEDURE

    [Fact]
    public void CreateFunction_RoundTrip()
    {
        // body 作为 token 流保留到第一个分号（对齐上游 captureFunctionBody 容器式行为）
        // 注意：含嵌套分号的复杂 body（BEGIN...END）需 lexer 状态机，当前不支持
        var stmt = SqlParser.Parse("CREATE FUNCTION my_func RETURNS INTEGER LANGUAGE SQL");

        Assert.NotNull(stmt);
        Assert.IsType<CreateFunction>(stmt);
        var fn = (CreateFunction)stmt;
        Assert.Equal("FUNCTION", fn.Kind);
        Assert.Contains("my_func", fn.FunctionDeclarationParts);
    }

    [Fact]
    public void CreateProcedure_RoundTrip()
    {
        var stmt = SqlParser.Parse("CREATE PROCEDURE my_proc LANGUAGE SQL");

        Assert.NotNull(stmt);
        Assert.IsType<CreateProcedure>(stmt);
        var proc = (CreateProcedure)stmt;
        Assert.Equal("PROCEDURE", proc.Kind);
        Assert.Contains("my_proc", proc.FunctionDeclarationParts);
    }

    [Fact]
    public void CreateFunction_OrReplace_ShouldSetFlag()
    {
        var stmt = SqlParser.Parse("CREATE OR REPLACE FUNCTION my_func RETURNS INTEGER LANGUAGE SQL");

        Assert.NotNull(stmt);
        var fn = Assert.IsType<CreateFunction>(stmt);
        Assert.True(fn.OrReplace);
    }

    #endregion

    #region BEGIN...END Block

    [Fact]
    public void Block_SimpleSelect_RoundTrip()
    {
        var stmt = SqlParser.Parse("BEGIN SELECT 1; END");

        Assert.NotNull(stmt);
        Assert.IsType<Azrng.JSqlParser.Statement.Block>(stmt);
    }

    [Fact]
    public void Block_MultipleStatements_ShouldParse()
    {
        var stmt = SqlParser.Parse("BEGIN SELECT 1; SELECT 2; END");

        Assert.NotNull(stmt);
        var block = Assert.IsType<Azrng.JSqlParser.Statement.Block>(stmt);
        Assert.Equal(2, block.Statements.StatementList.Count);
    }

    #endregion

    #region DECLARE

    [Fact]
    public void Declare_SimpleVariable_RoundTrip()
    {
        var stmt = SqlParser.Parse("DECLARE @x INT = 1");

        Assert.NotNull(stmt);
        Assert.IsType<Azrng.JSqlParser.Statement.DeclareStatement>(stmt);
    }

    [Fact]
    public void Declare_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("DECLARE @x INT");
        var declare = Assert.IsType<Azrng.JSqlParser.Statement.DeclareStatement>(stmt);

        Assert.Single(declare.TypeDefExprList);
        Assert.Equal("@x", declare.TypeDefExprList[0].UserVariable);
    }

    #endregion

    #region IF...ELSE

    [Fact]
    public void IfElse_WithElse_RoundTrip()
    {
        var stmt = SqlParser.Parse("IF 1 = 1 SELECT 1 ELSE SELECT 2");

        Assert.NotNull(stmt);
        Assert.IsType<Azrng.JSqlParser.Statement.IfElseStatement>(stmt);
    }

    [Fact]
    public void IfElse_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("IF 1 = 1 SELECT 1 ELSE SELECT 2");
        var ifElse = Assert.IsType<Azrng.JSqlParser.Statement.IfElseStatement>(stmt);

        Assert.NotNull(ifElse.Condition);
        Assert.NotNull(ifElse.IfStatement);
        Assert.NotNull(ifElse.ElseStatement);
    }

    [Fact]
    public void IfElse_WithoutElse_RoundTrip()
    {
        var stmt = SqlParser.Parse("IF 1 = 1 SELECT 1");

        Assert.NotNull(stmt);
        Assert.IsType<Azrng.JSqlParser.Statement.IfElseStatement>(stmt);
        var ifElse = Assert.IsType<Azrng.JSqlParser.Statement.IfElseStatement>(stmt);
        Assert.Null(ifElse.ElseStatement);
    }

    #endregion
}
