using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Cnf;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Util;

/// <summary>
/// 内部遍历引擎：把 visitor 的 push 模型桥接为 <see cref="ExpressionExtension.Descendants{T}"/> 所需的拉取式收集。
/// </summary>
/// <remarks>
/// <b>设计要点</b>：本类 <b>直接实现 <see cref="IExpressionVisitor{T}"/> 接口</b>，而非继承
/// <see cref="ExpressionVisitorAdapter{T}"/>。这样所有节点类型的 Visit 方法都由本类显式实现，
/// <b>编译器保证完整覆盖</b>——接口将来新增节点类型时，本类若漏实现会编译失败，强制补全，
/// 杜绝"某类节点静默不进 Descendants 结果"的契约违背。
/// <para>
/// 子节点递归路径对齐 ExpressionVisitorAdapter（已验证覆盖 Parenthesis/Function/Case/Between/In/
/// Analytic/Json/Array 等全部分支），见各 Visit 方法内的 <c>node.Accept(this, context)</c> 调用。
/// </para>
/// 不对外公开。对外 C# 风格入口见 <c>Azrng.JSqlParser.ExpressionExtension</c>。
/// </remarks>
internal sealed class ExpressionDescendantsWalker : IExpressionVisitor<object?>
{
    private readonly Action<IExpression> _onVisit;

    private ExpressionDescendantsWalker(Action<IExpression> onVisit)
    {
        _onVisit = onVisit;
    }

    /// <summary>从根表达式出发，按深度优先顺序回调每一个被访问到的表达式节点。</summary>
    public static void Walk(IExpression root, Action<IExpression> onVisit)
    {
        var walker = new ExpressionDescendantsWalker(onVisit);
        root.Accept(walker);
    }

    // 每个节点：先回调当前节点，再递归其子表达式（Accept(this, context)）。
    // 叶子节点（无子表达式）仅回调。
    // 子节点递归路径对齐 ExpressionVisitorAdapter（T098/M2 已验证）。

    // Value types（叶子）
    public object? Visit<S>(NullValue nullValue, S context) { _onVisit(nullValue); return default; }
    public object? Visit<S>(LongValue longValue, S context) { _onVisit(longValue); return default; }
    public object? Visit<S>(DoubleValue doubleValue, S context) { _onVisit(doubleValue); return default; }
    public object? Visit<S>(StringValue stringValue, S context) { _onVisit(stringValue); return default; }
    public object? Visit<S>(HexValue hexValue, S context) { _onVisit(hexValue); return default; }

    // Parameters（叶子）
    public object? Visit<S>(JdbcParameter jdbcParameter, S context) { _onVisit(jdbcParameter); return default; }
    public object? Visit<S>(JdbcNamedParameter jdbcNamedParameter, S context) { _onVisit(jdbcNamedParameter); return default; }

    // Basic expressions
    public object? Visit<S>(Parenthesis parenthesis, S context) { _onVisit(parenthesis); parenthesis.Expression.Accept(this, context); return default; }
    public object? Visit<S>(SignedExpression signedExpression, S context) { _onVisit(signedExpression); signedExpression.Expression.Accept(this, context); return default; }
    public object? Visit<S>(Function function, S context)
    {
        _onVisit(function);
        // M2 修复路径：遍历 Function 所有含子表达式的字段
        function.Parameters?.Accept(this, context);
        function.FilterExpression?.Accept(this, context);
        function.Separator?.Accept(this, context);
        if (function.NamedParameters?.Expressions != null)
            foreach (var e in function.NamedParameters.Expressions) e.Accept(this, context);
        VisitOrderByElements(function.OrderByElements, context);
        VisitOrderByElements(function.WithinGroupOrderByElements, context);
        if (function.KeywordArguments != null)
            foreach (var ka in function.KeywordArguments) ka.Expression?.Accept(this, context);
        function.Keep?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(CaseExpression caseExpression, S context)
    {
        _onVisit(caseExpression);
        caseExpression.SwitchExpression?.Accept(this, context);
        if (caseExpression.WhenClauses != null)
        {
            foreach (var wc in caseExpression.WhenClauses)
            {
                wc.WhenExpression?.Accept(this, context);
                wc.ThenExpression?.Accept(this, context);
            }
        }
        caseExpression.ElseExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(WhenClause whenClause, S context)
    {
        _onVisit(whenClause);
        whenClause.WhenExpression?.Accept(this, context);
        whenClause.ThenExpression?.Accept(this, context);
        return default;
    }

    // Arithmetic operators
    public object? Visit<S>(Addition addition, S context) { _onVisit(addition); return VisitBinary(addition, context); }
    public object? Visit<S>(Division division, S context) { _onVisit(division); return VisitBinary(division, context); }
    public object? Visit<S>(IntegerDivision division, S context) { _onVisit(division); return VisitBinary(division, context); }
    public object? Visit<S>(Multiplication multiplication, S context) { _onVisit(multiplication); return VisitBinary(multiplication, context); }
    public object? Visit<S>(Subtraction subtraction, S context) { _onVisit(subtraction); return VisitBinary(subtraction, context); }
    public object? Visit<S>(Modulo modulo, S context) { _onVisit(modulo); return VisitBinary(modulo, context); }
    public object? Visit<S>(Concat concat, S context) { _onVisit(concat); return VisitBinary(concat, context); }
    public object? Visit<S>(BitwiseAnd bitwiseAnd, S context) { _onVisit(bitwiseAnd); return VisitBinary(bitwiseAnd, context); }
    public object? Visit<S>(BitwiseOr bitwiseOr, S context) { _onVisit(bitwiseOr); return VisitBinary(bitwiseOr, context); }
    public object? Visit<S>(BitwiseXor bitwiseXor, S context) { _onVisit(bitwiseXor); return VisitBinary(bitwiseXor, context); }
    public object? Visit<S>(BitwiseLeftShift bitwiseLeftShift, S context) { _onVisit(bitwiseLeftShift); return VisitBinary(bitwiseLeftShift, context); }
    public object? Visit<S>(BitwiseRightShift bitwiseRightShift, S context) { _onVisit(bitwiseRightShift); return VisitBinary(bitwiseRightShift, context); }

    // Conditional operators
    public object? Visit<S>(AndExpression andExpression, S context) { _onVisit(andExpression); return VisitBinary(andExpression, context); }
    public object? Visit<S>(OrExpression orExpression, S context) { _onVisit(orExpression); return VisitBinary(orExpression, context); }
    public object? Visit<S>(XorExpression xorExpression, S context) { _onVisit(xorExpression); return VisitBinary(xorExpression, context); }
    public object? Visit<S>(NotExpression notExpression, S context) { _onVisit(notExpression); notExpression.Expression.Accept(this, context); return default; }

    // Relational operators
    public object? Visit<S>(EqualsTo equalsTo, S context) { _onVisit(equalsTo); return VisitBinary(equalsTo, context); }
    public object? Visit<S>(NotEqualsTo notEqualsTo, S context) { _onVisit(notEqualsTo); return VisitBinary(notEqualsTo, context); }
    public object? Visit<S>(GreaterThan greaterThan, S context) { _onVisit(greaterThan); return VisitBinary(greaterThan, context); }
    public object? Visit<S>(GreaterThanEquals greaterThanEquals, S context) { _onVisit(greaterThanEquals); return VisitBinary(greaterThanEquals, context); }
    public object? Visit<S>(MinorThan minorThan, S context) { _onVisit(minorThan); return VisitBinary(minorThan, context); }
    public object? Visit<S>(MinorThanEquals minorThanEquals, S context) { _onVisit(minorThanEquals); return VisitBinary(minorThanEquals, context); }
    public object? Visit<S>(Between between, S context)
    {
        _onVisit(between);
        between.LeftExpression.Accept(this, context);
        between.BetweenExpressionStart.Accept(this, context);
        between.BetweenExpressionEnd.Accept(this, context);
        return default;
    }
    public object? Visit<S>(InExpression inExpression, S context)
    {
        _onVisit(inExpression);
        inExpression.LeftExpression.Accept(this, context);
        inExpression.RightExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(IsNullExpression isNullExpression, S context) { _onVisit(isNullExpression); isNullExpression.LeftExpression?.Accept(this, context); return default; }
    public object? Visit<S>(IsBooleanExpression isBooleanExpression, S context) { _onVisit(isBooleanExpression); isBooleanExpression.LeftExpression?.Accept(this, context); return default; }
    public object? Visit<S>(IsDistinctExpression isDistinctExpression, S context) { _onVisit(isDistinctExpression); return VisitBinary(isDistinctExpression, context); }
    public object? Visit<S>(LikeExpression likeExpression, S context) { _onVisit(likeExpression); return VisitBinary(likeExpression, context); }
    public object? Visit<S>(ExistsExpression existsExpression, S context) { _onVisit(existsExpression); existsExpression.RightExpression?.Accept(this, context); return default; }
    public object? Visit<S>(FullTextSearch fullTextSearch, S context) { _onVisit(fullTextSearch); return default; }
    public object? Visit<S>(JsonOperator jsonOperator, S context) { _onVisit(jsonOperator); return VisitBinary(jsonOperator, context); }
    public object? Visit<S>(DoubleAnd doubleAnd, S context) { _onVisit(doubleAnd); return VisitBinary(doubleAnd, context); }
    public object? Visit<S>(Contains contains, S context) { _onVisit(contains); return VisitBinary(contains, context); }
    public object? Visit<S>(ContainedBy containedBy, S context) { _onVisit(containedBy); return VisitBinary(containedBy, context); }
    public object? Visit<S>(Matches matches, S context) { _onVisit(matches); return VisitBinary(matches, context); }
    public object? Visit<S>(RegExpMatchOperator regExpMatchOperator, S context) { _onVisit(regExpMatchOperator); return VisitBinary(regExpMatchOperator, context); }
    public object? Visit<S>(MemberOfExpression memberOfExpression, S context)
    {
        _onVisit(memberOfExpression);
        memberOfExpression.LeftExpression?.Accept(this, context);
        memberOfExpression.RightExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(OverlapsCondition overlapsCondition, S context)
    {
        _onVisit(overlapsCondition);
        overlapsCondition.LeftExpression?.Accept(this, context);
        overlapsCondition.RightExpression?.Accept(this, context);
        return default;
    }

    // Schema types（叶子）
    public object? Visit<S>(Column column, S context) { _onVisit(column); return default; }

    // Select-related
    public object? Visit<S>(AllColumns allColumns, S context) { _onVisit(allColumns); return default; }
    public object? Visit<S>(AllTableColumns allTableColumns, S context) { _onVisit(allTableColumns); return default; }
    public object? Visit<S>(Select select, S context) { _onVisit(select); return default; }

    // Advanced expressions
    public object? Visit<S>(CastExpression castExpression, S context) { _onVisit(castExpression); castExpression.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(AnalyticExpression analyticExpression, S context)
    {
        _onVisit(analyticExpression);
        analyticExpression.Expression?.Accept(this, context);
        analyticExpression.Offset?.Accept(this, context);
        analyticExpression.DefaultValue?.Accept(this, context);
        analyticExpression.FilterExpression?.Accept(this, context);
        if (analyticExpression.PartitionExpressionList != null)
            foreach (var e in analyticExpression.PartitionExpressionList) e.Accept(this, context);
        VisitOrderByElements(analyticExpression.OrderByElements, context);
        VisitOrderByElements(analyticExpression.WithinGroupOrderByElements, context);
        return default;
    }
    public object? Visit<S>(ExtractExpression extractExpression, S context) { _onVisit(extractExpression); extractExpression.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(IntervalExpression intervalExpression, S context) { _onVisit(intervalExpression); intervalExpression.Expression?.Accept(this, context); return default; }

    // JSqlParser 5.0 new expressions
    public object? Visit<S>(LambdaExpression lambdaExpression, S context) { _onVisit(lambdaExpression); lambdaExpression.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(StructType structType, S context) { _onVisit(structType); return default; }
    public object? Visit<S>(ExcludesExpression excludesExpression, S context)
    {
        _onVisit(excludesExpression);
        excludesExpression.LeftExpression?.Accept(this, context);
        excludesExpression.RightExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(IncludesExpression includesExpression, S context)
    {
        _onVisit(includesExpression);
        includesExpression.LeftExpression?.Accept(this, context);
        includesExpression.RightExpression?.Accept(this, context);
        return default;
    }

    // JSqlParser 5.1 new expressions
    public object? Visit<S>(BooleanValue booleanValue, S context) { _onVisit(booleanValue); return default; }
    public object? Visit<S>(DateTimeLiteralExpression dateTimeLiteralExpression, S context) { _onVisit(dateTimeLiteralExpression); return default; }
    public object? Visit<S>(ConnectByPriorOperator connectByPriorOperator, S context) { _onVisit(connectByPriorOperator); connectByPriorOperator.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(ConnectByRootOperator connectByRootOperator, S context) { _onVisit(connectByRootOperator); connectByRootOperator.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(HighExpression highExpression, S context) { _onVisit(highExpression); highExpression.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(LowExpression lowExpression, S context) { _onVisit(lowExpression); lowExpression.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(Inverse inverse, S context) { _onVisit(inverse); inverse.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(RangeExpression rangeExpression, S context)
    {
        _onVisit(rangeExpression);
        rangeExpression.StartExpression?.Accept(this, context);
        rangeExpression.EndExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(TimeKeyExpression timeKeyExpression, S context) { _onVisit(timeKeyExpression); return default; }
    public object? Visit<S>(TranscodingFunction transcodingFunction, S context) { _onVisit(transcodingFunction); transcodingFunction.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(JsonFunction jsonFunction, S context)
    {
        _onVisit(jsonFunction);
        foreach (var kvp in jsonFunction.KeyValuePairs)
        {
            if (kvp.Key is Expression.IExpression ke) ke.Accept(this, context);
            if (kvp.Value is Expression.IExpression ve) ve.Accept(this, context);
        }
        foreach (var expr in jsonFunction.Expressions) expr.Expression?.Accept(this, context);
        foreach (var expr in jsonFunction.PassingExpressions) expr.Accept(this, context);
        if (jsonFunction.InputExpression?.Expression != null) jsonFunction.InputExpression.Expression.Accept(this, context);
        if (jsonFunction.JsonPathExpression != null) jsonFunction.JsonPathExpression.Accept(this, context);
        if (jsonFunction.OnEmptyBehavior?.Expression != null) jsonFunction.OnEmptyBehavior.Expression.Accept(this, context);
        if (jsonFunction.OnErrorBehavior?.Expression != null) jsonFunction.OnErrorBehavior.Expression.Accept(this, context);
        return default;
    }
    public object? Visit<S>(JsonAggregateFunction jsonAggregateFunction, S context)
    {
        _onVisit(jsonAggregateFunction);
        jsonAggregateFunction.Value?.Accept(this, context);
        jsonAggregateFunction.AggregateExpression?.Accept(this, context);
        jsonAggregateFunction.FilterExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(CosineSimilarity cosineSimilarity, S context) { _onVisit(cosineSimilarity); return VisitBinary(cosineSimilarity, context); }
    public object? Visit<S>(GeometryDistance geometryDistance, S context) { _onVisit(geometryDistance); return VisitBinary(geometryDistance, context); }
    public object? Visit<S>(Plus plus, S context) { _onVisit(plus); return VisitBinary(plus, context); }
    public object? Visit<S>(PriorTo priorTo, S context) { _onVisit(priorTo); return VisitBinary(priorTo, context); }

    // JSqlParser 5.2 new expressions
    public object? Visit<S>(IsUnknownExpression isUnknownExpression, S context) { _onVisit(isUnknownExpression); isUnknownExpression.LeftExpression?.Accept(this, context); return default; }
    public object? Visit<S>(FunctionAllColumns functionAllColumns, S context) { _onVisit(functionAllColumns); return default; }

    // Collection / DIM expressions（此前 Adapter 未 override 导致漏覆盖，现统一补全）
    public object? Visit<S>(MultiAndExpression multiAndExpression, S context)
    {
        _onVisit(multiAndExpression);
        foreach (var expr in multiAndExpression.Expressions) expr.Accept(this, context);
        return default;
    }
    public object? Visit<S>(ExpressionList expressionList, S context)
    {
        _onVisit(expressionList);
        foreach (var expr in expressionList.Expressions) expr.Accept(this, context);
        return default;
    }
    public object? Visit<S>(RowGetExpression rowGetExpression, S context) { _onVisit(rowGetExpression); rowGetExpression.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(KeyExpression keyExpression, S context) { _onVisit(keyExpression); keyExpression.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(DateUnitExpression dateUnitExpression, S context) { _onVisit(dateUnitExpression); return default; }
    public object? Visit<S>(OracleNamedFunctionParameter oracleNamedFunctionParameter, S context) { _onVisit(oracleNamedFunctionParameter); oracleNamedFunctionParameter.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(PostgresNamedFunctionParameter postgresNamedFunctionParameter, S context) { _onVisit(postgresNamedFunctionParameter); postgresNamedFunctionParameter.Expression?.Accept(this, context); return default; }
    public object? Visit<S>(OracleHint oracleHint, S context) { _onVisit(oracleHint); return default; }
    public object? Visit<S>(KeepExpression keepExpression, S context)
    {
        _onVisit(keepExpression);
        VisitOrderByElements(keepExpression.OrderByElements, context);
        return default;
    }
    public object? Visit<S>(TrimFunction trimFunction, S context)
    {
        _onVisit(trimFunction);
        trimFunction.Expression?.Accept(this, context);
        trimFunction.FromExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(CollateExpression collateExpression, S context) { _onVisit(collateExpression); collateExpression.LeftExpression?.Accept(this, context); return default; }
    public object? Visit<S>(TimezoneExpression timezoneExpression, S context)
    {
        _onVisit(timezoneExpression);
        timezoneExpression.LeftExpression?.Accept(this, context);
        timezoneExpression.TimeZoneExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(NextValExpression nextValExpression, S context) { _onVisit(nextValExpression); return default; }
    public object? Visit<S>(AnyComparisonExpression anyComparisonExpression, S context) { _onVisit(anyComparisonExpression); return default; }
    public object? Visit<S>(ArrayConstructor arrayConstructor, S context)
    {
        _onVisit(arrayConstructor);
        if (arrayConstructor.Expressions?.Expressions != null)
            foreach (var expr in arrayConstructor.Expressions.Expressions) expr.Accept(this, context);
        return default;
    }
    public object? Visit<S>(ArrayExpression arrayExpression, S context)
    {
        _onVisit(arrayExpression);
        arrayExpression.ObjExpression?.Accept(this, context);
        arrayExpression.IndexExpression?.Accept(this, context);
        arrayExpression.StartIndexExpression?.Accept(this, context);
        arrayExpression.StopIndexExpression?.Accept(this, context);
        return default;
    }
    public object? Visit<S>(RowConstructor rowConstructor, S context)
    {
        _onVisit(rowConstructor);
        if (rowConstructor.Expressions?.Expressions != null)
            foreach (var expr in rowConstructor.Expressions.Expressions) expr.Accept(this, context);
        return default;
    }

    // ---------- 私有辅助 ----------

    private object? VisitBinary<S>(BinaryExpression binary, S context)
    {
        binary.LeftExpression.Accept(this, context);
        binary.RightExpression.Accept(this, context);
        return default;
    }

    private void VisitOrderByElements<S>(List<OrderByElement>? elements, S context)
    {
        if (elements == null) return;
        foreach (var ob in elements) ob.Expression?.Accept(this, context);
    }
}
