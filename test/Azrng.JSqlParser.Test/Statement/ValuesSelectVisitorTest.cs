using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Piped;
using Azrng.JSqlParser.Statement.Select;
using FromQuery = Azrng.JSqlParser.Statement.Piped.FromQuery;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// ISelectVisitor 调度测试（T097 补充）。
/// 验证 Values.Accept(ISelectVisitor) 正确触发 Visit(Values)，
/// 以及 SetOperationList 含 Values 时遍历调度正确。
/// </summary>
public class ValuesSelectVisitorTest
{
    /// <summary>
    /// 记录被访问的 Select 子类型序列的最小 ISelectVisitor 实现。
    /// ISelectVisitor&lt;T&gt; 无 Adapter 基类，需实现全部 6 个泛型方法。
    /// </summary>
    private class SelectTypeRecorder : ISelectVisitor<object?>
    {
        public List<string> VisitedTypes { get; } = new();

        public object? Visit<S>(PlainSelect plainSelect, S context)
        {
            VisitedTypes.Add("PlainSelect");
            return null;
        }
        public object? Visit<S>(SetOperationList setOpList, S context)
        {
            VisitedTypes.Add("SetOperationList");
            // 遍历集合元素，触发子 Select 调度
            foreach (var s in setOpList.Selects) s.Accept(this, context);
            return null;
        }
        public object? Visit<S>(WithItem withItem, S context)
        {
            VisitedTypes.Add("WithItem");
            return null;
        }
        public object? Visit<S>(FromQuery fromQuery, S context)
        {
            VisitedTypes.Add("FromQuery");
            return null;
        }
        public object? Visit<S>(TableStatement tableStatement, S context)
        {
            VisitedTypes.Add("TableStatement");
            return null;
        }
        public object? Visit<S>(Values values, S context)
        {
            VisitedTypes.Add("Values");
            return null;
        }
    }

    /// <summary>独立 Values 语句经 ISelectVisitor 调度应触发 Visit(Values)。</summary>
    [Fact]
    public void SelectVisitor_StandaloneValues_DispatchesToVisitValues()
    {
        var stmt = (Select)SqlParser.Parse("VALUES (1, 'a'), (2, 'b')")!;
        var visitor = new SelectTypeRecorder();

        stmt.Accept(visitor, (object?)null);

        Assert.Equal(new[] { "Values" }, visitor.VisitedTypes);
    }

    /// <summary>VALUES UNION VALUES 时，SetOperationList 调度应遍历到两个 Values 子节点。</summary>
    [Fact]
    public void SelectVisitor_ValuesUnionValues_DispatchesToBothValues()
    {
        var stmt = (Select)SqlParser.Parse("VALUES (1) UNION VALUES (2)")!;
        var visitor = new SelectTypeRecorder();

        stmt.Accept(visitor, (object?)null);

        // SetOperationList 先被访问，再遍历两个 Values
        Assert.Equal(new[] { "SetOperationList", "Values", "Values" }, visitor.VisitedTypes);
    }

    /// <summary>ISelectVisitor 的无参便利重载 Visit(Values) 可用且转发到泛型方法。
    /// 注：default 接口方法需通过接口类型引用调用。</summary>
    [Fact]
    public void SelectVisitor_ConvenienceOverload_VisitValues_Works()
    {
        var stmt = (Values)SqlParser.Parse("VALUES (1)")!;
        ISelectVisitor<object?> visitor = new SelectTypeRecorder();

        // 无参便利重载（default 接口方法）
        visitor.Visit(stmt);

        Assert.Contains("Values", ((SelectTypeRecorder)visitor).VisitedTypes);
    }
}
