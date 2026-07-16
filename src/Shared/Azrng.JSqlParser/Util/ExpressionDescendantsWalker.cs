using Azrng.JSqlParser.Expression.Cnf;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Expression;

using JExpression = Azrng.JSqlParser.Expression.Expression;

namespace Azrng.JSqlParser.Util;

/// <summary>
/// 内部遍历引擎：把 visitor 的 push 模型桥接为 <see cref="ExpressionExtension.Descendants{T}"/> 所需的拉取式收集。
/// 每个 Visit 方法统一为「回调当前节点 + 复用 <see cref="ExpressionVisitorAdapter{T}"/> 已验证的子节点递归」，
/// 保证覆盖范围与 Adapter 完全一致、不漏节点类型。
/// </summary>
/// <remarks>
/// 不对外公开。对外 C# 风格入口见 <c>Azrng.JSqlParser.ExpressionExtension</c>。
/// visitor 接口与 Adapter 是底层遍历机制（用于复杂自定义遍历与上游对照）。
/// </remarks>
internal sealed class ExpressionDescendantsWalker : ExpressionVisitorAdapter<object?>
{
    private readonly Action<JExpression> _onVisit;

    private ExpressionDescendantsWalker(Action<JExpression> onVisit)
    {
        _onVisit = onVisit;
    }

    /// <summary>从根表达式出发，按深度优先顺序回调每一个被访问到的表达式节点。</summary>
    public static void Walk(JExpression root, Action<JExpression> onVisit)
    {
        var walker = new ExpressionDescendantsWalker(onVisit);
        root.Accept(walker);
    }

    // 每个节点：先回调当前节点，再复用基类的子节点递归（base.Visit）。
    // base.Visit 对于叶子节点返回 default!（无子节点可递归），对于复合节点递归其子表达式。

    public override object? Visit<S>(NullValue nullValue, S context) { _onVisit(nullValue); return default; }
    public override object? Visit<S>(LongValue longValue, S context) { _onVisit(longValue); return default; }
    public override object? Visit<S>(DoubleValue doubleValue, S context) { _onVisit(doubleValue); return default; }
    public override object? Visit<S>(StringValue stringValue, S context) { _onVisit(stringValue); return default; }
    public override object? Visit<S>(HexValue hexValue, S context) { _onVisit(hexValue); return default; }

    public override object? Visit<S>(JdbcParameter jdbcParameter, S context) { _onVisit(jdbcParameter); return default; }
    public override object? Visit<S>(JdbcNamedParameter jdbcNamedParameter, S context) { _onVisit(jdbcNamedParameter); return default; }

    public override object? Visit<S>(Parenthesis parenthesis, S context) { _onVisit(parenthesis); return base.Visit<S>(parenthesis, context); }
    public override object? Visit<S>(SignedExpression signedExpression, S context) { _onVisit(signedExpression); return base.Visit<S>(signedExpression, context); }
    public override object? Visit<S>(Function function, S context) { _onVisit(function); return base.Visit<S>(function, context); }
    public override object? Visit<S>(CaseExpression caseExpression, S context) { _onVisit(caseExpression); return base.Visit<S>(caseExpression, context); }
    public override object? Visit<S>(WhenClause whenClause, S context) { _onVisit(whenClause); return base.Visit<S>(whenClause, context); }

    // Arithmetic operators
    public override object? Visit<S>(Addition addition, S context) { _onVisit(addition); return base.Visit<S>(addition, context); }
    public override object? Visit<S>(Division division, S context) { _onVisit(division); return base.Visit<S>(division, context); }
    public override object? Visit<S>(IntegerDivision division, S context) { _onVisit(division); return base.Visit<S>(division, context); }
    public override object? Visit<S>(Multiplication multiplication, S context) { _onVisit(multiplication); return base.Visit<S>(multiplication, context); }
    public override object? Visit<S>(Subtraction subtraction, S context) { _onVisit(subtraction); return base.Visit<S>(subtraction, context); }
    public override object? Visit<S>(Modulo modulo, S context) { _onVisit(modulo); return base.Visit<S>(modulo, context); }
    public override object? Visit<S>(Concat concat, S context) { _onVisit(concat); return base.Visit<S>(concat, context); }
    public override object? Visit<S>(BitwiseAnd bitwiseAnd, S context) { _onVisit(bitwiseAnd); return base.Visit<S>(bitwiseAnd, context); }
    public override object? Visit<S>(BitwiseOr bitwiseOr, S context) { _onVisit(bitwiseOr); return base.Visit<S>(bitwiseOr, context); }
    public override object? Visit<S>(BitwiseXor bitwiseXor, S context) { _onVisit(bitwiseXor); return base.Visit<S>(bitwiseXor, context); }
    public override object? Visit<S>(BitwiseLeftShift bitwiseLeftShift, S context) { _onVisit(bitwiseLeftShift); return base.Visit<S>(bitwiseLeftShift, context); }
    public override object? Visit<S>(BitwiseRightShift bitwiseRightShift, S context) { _onVisit(bitwiseRightShift); return base.Visit<S>(bitwiseRightShift, context); }

    // Conditional operators
    public override object? Visit<S>(AndExpression andExpression, S context) { _onVisit(andExpression); return base.Visit<S>(andExpression, context); }
    public override object? Visit<S>(OrExpression orExpression, S context) { _onVisit(orExpression); return base.Visit<S>(orExpression, context); }
    public override object? Visit<S>(XorExpression xorExpression, S context) { _onVisit(xorExpression); return base.Visit<S>(xorExpression, context); }
    public override object? Visit<S>(NotExpression notExpression, S context) { _onVisit(notExpression); return base.Visit<S>(notExpression, context); }

    // Relational operators
    public override object? Visit<S>(EqualsTo equalsTo, S context) { _onVisit(equalsTo); return base.Visit<S>(equalsTo, context); }
    public override object? Visit<S>(NotEqualsTo notEqualsTo, S context) { _onVisit(notEqualsTo); return base.Visit<S>(notEqualsTo, context); }
    public override object? Visit<S>(GreaterThan greaterThan, S context) { _onVisit(greaterThan); return base.Visit<S>(greaterThan, context); }
    public override object? Visit<S>(GreaterThanEquals greaterThanEquals, S context) { _onVisit(greaterThanEquals); return base.Visit<S>(greaterThanEquals, context); }
    public override object? Visit<S>(MinorThan minorThan, S context) { _onVisit(minorThan); return base.Visit<S>(minorThan, context); }
    public override object? Visit<S>(MinorThanEquals minorThanEquals, S context) { _onVisit(minorThanEquals); return base.Visit<S>(minorThanEquals, context); }
    public override object? Visit<S>(Between between, S context) { _onVisit(between); return base.Visit<S>(between, context); }
    public override object? Visit<S>(InExpression inExpression, S context) { _onVisit(inExpression); return base.Visit<S>(inExpression, context); }
    public override object? Visit<S>(IsNullExpression isNullExpression, S context) { _onVisit(isNullExpression); return base.Visit<S>(isNullExpression, context); }
    public override object? Visit<S>(IsBooleanExpression isBooleanExpression, S context) { _onVisit(isBooleanExpression); return default; }
    public override object? Visit<S>(IsDistinctExpression isDistinctExpression, S context) { _onVisit(isDistinctExpression); return default; }
    public override object? Visit<S>(LikeExpression likeExpression, S context) { _onVisit(likeExpression); return base.Visit<S>(likeExpression, context); }
    public override object? Visit<S>(SimilarToExpression similarToExpression, S context) { _onVisit(similarToExpression); return default; }
    public override object? Visit<S>(ExistsExpression existsExpression, S context) { _onVisit(existsExpression); return base.Visit<S>(existsExpression, context); }
    public override object? Visit<S>(FullTextSearch fullTextSearch, S context) { _onVisit(fullTextSearch); return default; }
    public override object? Visit<S>(JsonOperator jsonOperator, S context) { _onVisit(jsonOperator); return default; }
    public override object? Visit<S>(DoubleAnd doubleAnd, S context) { _onVisit(doubleAnd); return base.Visit<S>(doubleAnd, context); }
    public override object? Visit<S>(Contains contains, S context) { _onVisit(contains); return default; }
    public override object? Visit<S>(ContainedBy containedBy, S context) { _onVisit(containedBy); return default; }
    public override object? Visit<S>(Matches matches, S context) { _onVisit(matches); return default; }
    public override object? Visit<S>(RegExpMatchOperator regExpMatchOperator, S context) { _onVisit(regExpMatchOperator); return default; }
    public override object? Visit<S>(MemberOfExpression memberOfExpression, S context) { _onVisit(memberOfExpression); return base.Visit<S>(memberOfExpression, context); }
    public override object? Visit<S>(OverlapsCondition overlapsCondition, S context) { _onVisit(overlapsCondition); return base.Visit<S>(overlapsCondition, context); }

    public override object? Visit<S>(Column column, S context) { _onVisit(column); return default; }

    // Select-related
    public override object? Visit<S>(Statement.Select.AllColumns allColumns, S context) { _onVisit(allColumns); return default; }
    public override object? Visit<S>(Statement.Select.AllTableColumns allTableColumns, S context) { _onVisit(allTableColumns); return default; }
    public override object? Visit<S>(Statement.Select.Select select, S context) { _onVisit(select); return default; }

    // Advanced expressions
    public override object? Visit<S>(CastExpression castExpression, S context) { _onVisit(castExpression); return base.Visit<S>(castExpression, context); }
    public override object? Visit<S>(AnalyticExpression analyticExpression, S context) { _onVisit(analyticExpression); return base.Visit<S>(analyticExpression, context); }
    public override object? Visit<S>(ExtractExpression extractExpression, S context) { _onVisit(extractExpression); return base.Visit<S>(extractExpression, context); }
    public override object? Visit<S>(IntervalExpression intervalExpression, S context) { _onVisit(intervalExpression); return base.Visit<S>(intervalExpression, context); }

    // JSqlParser 5.0 new expressions
    public override object? Visit<S>(LambdaExpression lambdaExpression, S context) { _onVisit(lambdaExpression); return base.Visit<S>(lambdaExpression, context); }
    public override object? Visit<S>(StructType structType, S context) { _onVisit(structType); return default; }
    public override object? Visit<S>(ExcludesExpression excludesExpression, S context) { _onVisit(excludesExpression); return base.Visit<S>(excludesExpression, context); }
    public override object? Visit<S>(IncludesExpression includesExpression, S context) { _onVisit(includesExpression); return base.Visit<S>(includesExpression, context); }

    // JSqlParser 5.1 new expressions
    public override object? Visit<S>(BooleanValue booleanValue, S context) { _onVisit(booleanValue); return default; }
    public override object? Visit<S>(DateTimeLiteralExpression dateTimeLiteralExpression, S context) { _onVisit(dateTimeLiteralExpression); return default; }
    public override object? Visit<S>(ConnectByPriorOperator connectByPriorOperator, S context) { _onVisit(connectByPriorOperator); return base.Visit<S>(connectByPriorOperator, context); }
    public override object? Visit<S>(ConnectByRootOperator connectByRootOperator, S context) { _onVisit(connectByRootOperator); return base.Visit<S>(connectByRootOperator, context); }
    public override object? Visit<S>(HighExpression highExpression, S context) { _onVisit(highExpression); return base.Visit<S>(highExpression, context); }
    public override object? Visit<S>(LowExpression lowExpression, S context) { _onVisit(lowExpression); return base.Visit<S>(lowExpression, context); }
    public override object? Visit<S>(Inverse inverse, S context) { _onVisit(inverse); return base.Visit<S>(inverse, context); }
    public override object? Visit<S>(RangeExpression rangeExpression, S context) { _onVisit(rangeExpression); return base.Visit<S>(rangeExpression, context); }
    public override object? Visit<S>(TimeKeyExpression timeKeyExpression, S context) { _onVisit(timeKeyExpression); return default; }
    public override object? Visit<S>(TranscodingFunction transcodingFunction, S context) { _onVisit(transcodingFunction); return base.Visit<S>(transcodingFunction, context); }
    public override object? Visit<S>(JsonFunction jsonFunction, S context) { _onVisit(jsonFunction); return base.Visit<S>(jsonFunction, context); }
    public override object? Visit<S>(JsonAggregateFunction jsonAggregateFunction, S context) { _onVisit(jsonAggregateFunction); return base.Visit<S>(jsonAggregateFunction, context); }
    public override object? Visit<S>(CosineSimilarity cosineSimilarity, S context) { _onVisit(cosineSimilarity); return base.Visit<S>(cosineSimilarity, context); }
    public override object? Visit<S>(GeometryDistance geometryDistance, S context) { _onVisit(geometryDistance); return base.Visit<S>(geometryDistance, context); }
    public override object? Visit<S>(Plus plus, S context) { _onVisit(plus); return base.Visit<S>(plus, context); }
    public override object? Visit<S>(PriorTo priorTo, S context) { _onVisit(priorTo); return base.Visit<S>(priorTo, context); }

    // JSqlParser 5.2 new expressions
    public override object? Visit<S>(IsUnknownExpression isUnknownExpression, S context) { _onVisit(isUnknownExpression); return base.Visit<S>(isUnknownExpression, context); }
    public override object? Visit<S>(Statement.Select.FunctionAllColumns functionAllColumns, S context) { _onVisit(functionAllColumns); return default; }

    // Collection / DIM expressions —— Adapter 已 override 这些（递归子节点），此处补回调。
    public override object? Visit<S>(MultiAndExpression multiAndExpression, S context) { _onVisit(multiAndExpression); return base.Visit<S>(multiAndExpression, context); }
    public override object? Visit<S>(ExpressionList expressionList, S context) { _onVisit(expressionList); return base.Visit<S>(expressionList, context); }
    public override object? Visit<S>(RowGetExpression rowGetExpression, S context) { _onVisit(rowGetExpression); return base.Visit<S>(rowGetExpression, context); }
    public override object? Visit<S>(KeyExpression keyExpression, S context) { _onVisit(keyExpression); return base.Visit<S>(keyExpression, context); }
    public override object? Visit<S>(KeepExpression keepExpression, S context) { _onVisit(keepExpression); return base.Visit<S>(keepExpression, context); }

    // 注意：DateUnitExpression/OracleHint/TrimFunction/CollateExpression/TimezoneExpression/
    // NextValExpression/AnyComparisonExpression/ArrayConstructor/ArrayExpression/RowConstructor/
    // OracleNamedFunctionParameter/PostgresNamedFunctionParameter 等仅有接口默认实现（Adapter 未 override），
    // 无法在此 override。它们的子节点仍会通过接口默认实现被递归访问到（因此 Descendants<Column> 等子节点收集不受影响），
    // 仅这些边缘父节点本身不会出现在 Descendants 结果中。
}
