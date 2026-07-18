using Azrng.JSqlParser.Expression.Cnf;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Base adapter for IExpressionVisitor with virtual implementations that
/// recursively visit child expressions. Override specific Visit methods
/// to customize behavior.
/// 注意：所有子表达式遍历统一透传 context（M1 修复），避免派生 visitor 丢失上下文。
/// </summary>
public class ExpressionVisitorAdapter<T> : IExpressionVisitor<T>
{
    public virtual T Visit<S>(NullValue nullValue, S context) => default!;
    public virtual T Visit<S>(LongValue longValue, S context) => default!;
    public virtual T Visit<S>(DoubleValue doubleValue, S context) => default!;
    public virtual T Visit<S>(StringValue stringValue, S context) => default!;
    public virtual T Visit<S>(HexValue hexValue, S context) => default!;

    public virtual T Visit<S>(JdbcParameter jdbcParameter, S context) => default!;
    public virtual T Visit<S>(JdbcNamedParameter jdbcNamedParameter, S context) => default!;

    public virtual T Visit<S>(Parenthesis parenthesis, S context) { parenthesis.Expression.Accept(this, context); return default!; }
    public virtual T Visit<S>(SignedExpression signedExpression, S context) { signedExpression.Expression.Accept(this, context); return default!; }
    public virtual T Visit<S>(Function function, S context)
    {
        // M2 修复：遍历 Function 所有含子表达式的字段
        function.Parameters?.Accept(this, context);
        function.FilterExpression?.Accept(this, context);
        function.Separator?.Accept(this, context);
        if (function.NamedParameters?.Expressions != null)
        {
            foreach (var e in function.NamedParameters.Expressions) e.Accept(this, context);
        }
        VisitOrderByElements(function.OrderByElements, context);
        VisitOrderByElements(function.WithinGroupOrderByElements, context);
        if (function.KeywordArguments != null)
        {
            foreach (var ka in function.KeywordArguments) ka.Expression?.Accept(this, context);
        }
        function.Keep?.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(CaseExpression caseExpression, S context)
    {
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
        return default!;
    }
    public virtual T Visit<S>(WhenClause whenClause, S context) { whenClause.WhenExpression?.Accept(this, context); whenClause.ThenExpression?.Accept(this, context); return default!; }

    // Arithmetic operators - recurse into children
    public virtual T Visit<S>(Addition addition, S context) => VisitBinary(addition, context);
    public virtual T Visit<S>(Division division, S context) => VisitBinary(division, context);
    public virtual T Visit<S>(IntegerDivision division, S context) => VisitBinary(division, context);
    public virtual T Visit<S>(Multiplication multiplication, S context) => VisitBinary(multiplication, context);
    public virtual T Visit<S>(Subtraction subtraction, S context) => VisitBinary(subtraction, context);
    public virtual T Visit<S>(Modulo modulo, S context) => VisitBinary(modulo, context);
    public virtual T Visit<S>(Concat concat, S context) => VisitBinary(concat, context);
    public virtual T Visit<S>(BitwiseAnd bitwiseAnd, S context) => VisitBinary(bitwiseAnd, context);
    public virtual T Visit<S>(BitwiseOr bitwiseOr, S context) => VisitBinary(bitwiseOr, context);
    public virtual T Visit<S>(BitwiseXor bitwiseXor, S context) => VisitBinary(bitwiseXor, context);
    public virtual T Visit<S>(BitwiseLeftShift bitwiseLeftShift, S context) => VisitBinary(bitwiseLeftShift, context);
    public virtual T Visit<S>(BitwiseRightShift bitwiseRightShift, S context) => VisitBinary(bitwiseRightShift, context);

    // Conditional operators - recurse into children
    public virtual T Visit<S>(AndExpression andExpression, S context) => VisitBinary(andExpression, context);
    public virtual T Visit<S>(OrExpression orExpression, S context) => VisitBinary(orExpression, context);
    public virtual T Visit<S>(XorExpression xorExpression, S context) => VisitBinary(xorExpression, context);
    public virtual T Visit<S>(NotExpression notExpression, S context) { notExpression.Expression.Accept(this, context); return default!; }

    // Relational operators - recurse into children
    public virtual T Visit<S>(EqualsTo equalsTo, S context) => VisitBinary(equalsTo, context);
    public virtual T Visit<S>(NotEqualsTo notEqualsTo, S context) => VisitBinary(notEqualsTo, context);
    public virtual T Visit<S>(GreaterThan greaterThan, S context) => VisitBinary(greaterThan, context);
    public virtual T Visit<S>(GreaterThanEquals greaterThanEquals, S context) => VisitBinary(greaterThanEquals, context);
    public virtual T Visit<S>(MinorThan minorThan, S context) => VisitBinary(minorThan, context);
    public virtual T Visit<S>(MinorThanEquals minorThanEquals, S context) => VisitBinary(minorThanEquals, context);
    public virtual T Visit<S>(Between between, S context)
    {
        between.LeftExpression.Accept(this, context);
        between.BetweenExpressionStart.Accept(this, context);
        between.BetweenExpressionEnd.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(InExpression inExpression, S context)
    {
        inExpression.LeftExpression.Accept(this, context);
        inExpression.RightExpression?.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(IsNullExpression isNullExpression, S context) { isNullExpression.LeftExpression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(IsBooleanExpression isBooleanExpression, S context) { isBooleanExpression.LeftExpression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(IsDistinctExpression isDistinctExpression, S context) => VisitBinary(isDistinctExpression, context);
    public virtual T Visit<S>(LikeExpression likeExpression, S context) => VisitBinary(likeExpression, context);
    public virtual T Visit<S>(ExistsExpression existsExpression, S context) { existsExpression.RightExpression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(FullTextSearch fullTextSearch, S context) => default!;
    public virtual T Visit<S>(JsonOperator jsonOperator, S context) => VisitBinary(jsonOperator, context);
    public virtual T Visit<S>(DoubleAnd doubleAnd, S context) => VisitBinary(doubleAnd, context);
    public virtual T Visit<S>(Contains contains, S context) => VisitBinary(contains, context);
    public virtual T Visit<S>(ContainedBy containedBy, S context) => VisitBinary(containedBy, context);
    public virtual T Visit<S>(Matches matches, S context) => VisitBinary(matches, context);
    public virtual T Visit<S>(RegExpMatchOperator regExpMatchOperator, S context) => VisitBinary(regExpMatchOperator, context);
    public virtual T Visit<S>(MemberOfExpression memberOfExpression, S context) { memberOfExpression.LeftExpression?.Accept(this, context); memberOfExpression.RightExpression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(OverlapsCondition overlapsCondition, S context) { overlapsCondition.LeftExpression?.Accept(this, context); overlapsCondition.RightExpression?.Accept(this, context); return default!; }

    public virtual T Visit<S>(Column column, S context) => default!;

    public virtual T Visit<S>(AllColumns allColumns, S context) => default!;
    public virtual T Visit<S>(AllTableColumns allTableColumns, S context) => default!;
    public virtual T Visit<S>(Select select, S context) => default!;

    public virtual T Visit<S>(CastExpression castExpression, S context) { castExpression.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(AnalyticExpression analyticExpression, S context)
    {
        // M2 修复：AnalyticExpression 原先完全不遍历，补全子表达式
        analyticExpression.Expression?.Accept(this, context);
        analyticExpression.Offset?.Accept(this, context);
        analyticExpression.DefaultValue?.Accept(this, context);
        analyticExpression.FilterExpression?.Accept(this, context);
        if (analyticExpression.PartitionExpressionList != null)
        {
            foreach (var e in analyticExpression.PartitionExpressionList) e.Accept(this, context);
        }
        VisitOrderByElements(analyticExpression.OrderByElements, context);
        VisitOrderByElements(analyticExpression.WithinGroupOrderByElements, context);
        return default!;
    }
    public virtual T Visit<S>(ExtractExpression extractExpression, S context) { extractExpression.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(IntervalExpression intervalExpression, S context) { intervalExpression.Expression?.Accept(this, context); return default!; }

    // JSqlParser 5.0 new expressions
    public virtual T Visit<S>(LambdaExpression lambdaExpression, S context) { lambdaExpression.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(StructType structType, S context) => default!;
    public virtual T Visit<S>(Operators.Relational.ExcludesExpression excludesExpression, S context) { excludesExpression.LeftExpression?.Accept(this, context); excludesExpression.RightExpression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(Operators.Relational.IncludesExpression includesExpression, S context) { includesExpression.LeftExpression?.Accept(this, context); includesExpression.RightExpression?.Accept(this, context); return default!; }

    // JSqlParser 5.1 new expressions
    public virtual T Visit<S>(BooleanValue booleanValue, S context) => default!;
    public virtual T Visit<S>(DateTimeLiteralExpression dateTimeLiteralExpression, S context) => default!;
    public virtual T Visit<S>(ConnectByPriorOperator connectByPriorOperator, S context) { connectByPriorOperator.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(ConnectByRootOperator connectByRootOperator, S context) { connectByRootOperator.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(HighExpression highExpression, S context) { highExpression.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(LowExpression lowExpression, S context) { lowExpression.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(Inverse inverse, S context) { inverse.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(RangeExpression rangeExpression, S context)
    {
        rangeExpression.StartExpression?.Accept(this, context);
        rangeExpression.EndExpression?.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(TimeKeyExpression timeKeyExpression, S context) => default!;
    public virtual T Visit<S>(TranscodingFunction transcodingFunction, S context)
    {
        transcodingFunction.Expression?.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(JsonFunction jsonFunction, S context)
    {
        foreach (var kvp in jsonFunction.KeyValuePairs)
        {
            if (kvp.Key is IExpression ke) ke.Accept(this, context);
            if (kvp.Value is IExpression ve) ve.Accept(this, context);
        }
        foreach (var expr in jsonFunction.Expressions)
        {
            expr.Expression?.Accept(this, context);
        }
        foreach (var expr in jsonFunction.PassingExpressions)
        {
            expr.Accept(this, context);
        }
        if (jsonFunction.InputExpression?.Expression != null) jsonFunction.InputExpression.Expression.Accept(this, context);
        if (jsonFunction.JsonPathExpression != null) jsonFunction.JsonPathExpression.Accept(this, context);
        if (jsonFunction.OnEmptyBehavior?.Expression != null) jsonFunction.OnEmptyBehavior.Expression.Accept(this, context);
        if (jsonFunction.OnErrorBehavior?.Expression != null) jsonFunction.OnErrorBehavior.Expression.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(JsonAggregateFunction jsonAggregateFunction, S context)
    {
        jsonAggregateFunction.Value?.Accept(this, context);
        jsonAggregateFunction.AggregateExpression?.Accept(this, context);
        jsonAggregateFunction.FilterExpression?.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(Operators.Relational.CosineSimilarity cosineSimilarity, S context) => VisitBinary(cosineSimilarity, context);
    public virtual T Visit<S>(Operators.Relational.GeometryDistance geometryDistance, S context) => VisitBinary(geometryDistance, context);
    public virtual T Visit<S>(Operators.Relational.Plus plus, S context) => VisitBinary(plus, context);
    public virtual T Visit<S>(Operators.Relational.PriorTo priorTo, S context) => VisitBinary(priorTo, context);

    // JSqlParser 5.2 new expressions
    public virtual T Visit<S>(Operators.Relational.IsUnknownExpression isUnknownExpression, S context) { isUnknownExpression.LeftExpression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(Statement.Select.FunctionAllColumns functionAllColumns, S context) => default!;

    // Collection expressions
    public virtual T Visit<S>(MultiAndExpression multiAndExpression, S context) { foreach (var expr in multiAndExpression.Expressions) expr.Accept(this, context); return default!; }
    public virtual T Visit<S>(ExpressionList expressionList, S context) { foreach (var expr in expressionList.Expressions) expr.Accept(this, context); return default!; }
    public virtual T Visit<S>(RowGetExpression rowGetExpression, S context) { rowGetExpression.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(KeyExpression keyExpression, S context) { keyExpression.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(KeepExpression keepExpression, S context)
    {
        VisitOrderByElements(keepExpression.OrderByElements, context);
        return default!;
    }

    // 以下节点原先由接口 default method 提供递归（批 1 下沉至 Adapter，消除接口/Adapter 双份实现）
    public virtual T Visit<S>(DateUnitExpression dateUnitExpression, S context) => default!;
    public virtual T Visit<S>(OracleNamedFunctionParameter oracleNamedFunctionParameter, S context) { oracleNamedFunctionParameter.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(PostgresNamedFunctionParameter postgresNamedFunctionParameter, S context) { postgresNamedFunctionParameter.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(OracleHint oracleHint, S context) => default!;
    public virtual T Visit<S>(TrimFunction trimFunction, S context)
    {
        trimFunction.Expression?.Accept(this, context);
        trimFunction.FromExpression?.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(CollateExpression collateExpression, S context) { collateExpression.LeftExpression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(TimezoneExpression timezoneExpression, S context)
    {
        timezoneExpression.LeftExpression?.Accept(this, context);
        timezoneExpression.TimeZoneExpression?.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(NextValExpression nextValExpression, S context) => default!;
    public virtual T Visit<S>(AnyComparisonExpression anyComparisonExpression, S context) => default!;
    public virtual T Visit<S>(ArrayConstructor arrayConstructor, S context)
    {
        if (arrayConstructor.Expressions?.Expressions != null)
        {
            foreach (var expr in arrayConstructor.Expressions.Expressions) expr.Accept(this, context);
        }
        return default!;
    }
    public virtual T Visit<S>(ArrayExpression arrayExpression, S context)
    {
        arrayExpression.ObjExpression?.Accept(this, context);
        arrayExpression.IndexExpression?.Accept(this, context);
        arrayExpression.StartIndexExpression?.Accept(this, context);
        arrayExpression.StopIndexExpression?.Accept(this, context);
        return default!;
    }
    public virtual T Visit<S>(RowConstructor rowConstructor, S context)
    {
        if (rowConstructor.Expressions?.Expressions != null)
        {
            foreach (var expr in rowConstructor.Expressions.Expressions) expr.Accept(this, context);
        }
        return default!;
    }

    private T VisitBinary<S>(BinaryExpression binary, S context)
    {
        binary.LeftExpression.Accept(this, context);
        binary.RightExpression.Accept(this, context);
        return default!;
    }

    /// <summary>遍历 OrderByElement 列表中的子表达式。</summary>
    private void VisitOrderByElements<S>(List<OrderByElement>? elements, S context)
    {
        if (elements == null) return;
        foreach (var ob in elements) ob.Expression?.Accept(this, context);
    }
}
