using Azrng.JSqlParser.Expression.Cnf;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Base adapter for ExpressionVisitor with virtual implementations that
/// recursively visit child expressions. Override specific Visit methods
/// to customize behavior.
/// </summary>
public class ExpressionVisitorAdapter<T> : ExpressionVisitor<T>
{
    public virtual T Visit<S>(NullValue nullValue, S context) => default!;
    public virtual T Visit<S>(LongValue longValue, S context) => default!;
    public virtual T Visit<S>(DoubleValue doubleValue, S context) => default!;
    public virtual T Visit<S>(StringValue stringValue, S context) => default!;
    public virtual T Visit<S>(DateValue dateValue, S context) => default!;
    public virtual T Visit<S>(TimeValue timeValue, S context) => default!;
    public virtual T Visit<S>(TimestampValue timestampValue, S context) => default!;
    public virtual T Visit<S>(HexValue hexValue, S context) => default!;

    public virtual T Visit<S>(JdbcParameter jdbcParameter, S context) => default!;
    public virtual T Visit<S>(JdbcNamedParameter jdbcNamedParameter, S context) => default!;

    public virtual T Visit<S>(Parenthesis parenthesis, S context) { parenthesis.Expression.Accept(this); return default!; }
    public virtual T Visit<S>(SignedExpression signedExpression, S context) { signedExpression.Expression.Accept(this); return default!; }
    public virtual T Visit<S>(Function function, S context) { function.Parameters?.Accept(this); return default!; }
    public virtual T Visit<S>(CaseExpression caseExpression, S context) => default!;
    public virtual T Visit<S>(WhenClause whenClause, S context) => default!;

    // Arithmetic operators - recurse into children
    public virtual T Visit<S>(Addition addition, S context) => VisitBinary(addition);
    public virtual T Visit<S>(Division division, S context) => VisitBinary(division);
    public virtual T Visit<S>(IntegerDivision division, S context) => VisitBinary(division);
    public virtual T Visit<S>(Multiplication multiplication, S context) => VisitBinary(multiplication);
    public virtual T Visit<S>(Subtraction subtraction, S context) => VisitBinary(subtraction);
    public virtual T Visit<S>(Modulo modulo, S context) => VisitBinary(modulo);
    public virtual T Visit<S>(Concat concat, S context) => VisitBinary(concat);
    public virtual T Visit<S>(BitwiseAnd bitwiseAnd, S context) => VisitBinary(bitwiseAnd);
    public virtual T Visit<S>(BitwiseOr bitwiseOr, S context) => VisitBinary(bitwiseOr);
    public virtual T Visit<S>(BitwiseXor bitwiseXor, S context) => VisitBinary(bitwiseXor);
    public virtual T Visit<S>(BitwiseLeftShift bitwiseLeftShift, S context) => VisitBinary(bitwiseLeftShift);
    public virtual T Visit<S>(BitwiseRightShift bitwiseRightShift, S context) => VisitBinary(bitwiseRightShift);

    // Conditional operators - recurse into children
    public virtual T Visit<S>(AndExpression andExpression, S context) => VisitBinary(andExpression);
    public virtual T Visit<S>(OrExpression orExpression, S context) => VisitBinary(orExpression);
    public virtual T Visit<S>(XorExpression xorExpression, S context) => VisitBinary(xorExpression);
    public virtual T Visit<S>(NotExpression notExpression, S context) { notExpression.Expression.Accept(this); return default!; }

    // Relational operators - recurse into children
    public virtual T Visit<S>(EqualsTo equalsTo, S context) => VisitBinary(equalsTo);
    public virtual T Visit<S>(NotEqualsTo notEqualsTo, S context) => VisitBinary(notEqualsTo);
    public virtual T Visit<S>(GreaterThan greaterThan, S context) => VisitBinary(greaterThan);
    public virtual T Visit<S>(GreaterThanEquals greaterThanEquals, S context) => VisitBinary(greaterThanEquals);
    public virtual T Visit<S>(MinorThan minorThan, S context) => VisitBinary(minorThan);
    public virtual T Visit<S>(MinorThanEquals minorThanEquals, S context) => VisitBinary(minorThanEquals);
    public virtual T Visit<S>(Between between, S context)
    {
        between.LeftExpression.Accept(this);
        between.BetweenExpressionStart.Accept(this);
        between.BetweenExpressionEnd.Accept(this);
        return default!;
    }
    public virtual T Visit<S>(InExpression inExpression, S context)
    {
        inExpression.LeftExpression.Accept(this);
        inExpression.RightExpression.Accept(this);
        return default!;
    }
    public virtual T Visit<S>(IsNullExpression isNullExpression, S context) { isNullExpression.LeftExpression.Accept(this); return default!; }
    public virtual T Visit<S>(IsBooleanExpression isBooleanExpression, S context) => default!;
    public virtual T Visit<S>(IsDistinctExpression isDistinctExpression, S context) => default!;
    public virtual T Visit<S>(LikeExpression likeExpression, S context) => VisitBinary(likeExpression);
    public virtual T Visit<S>(SimilarToExpression similarToExpression, S context) => default!;
    public virtual T Visit<S>(ExistsExpression existsExpression, S context) { existsExpression.RightExpression.Accept(this); return default!; }
    public virtual T Visit<S>(FullTextSearch fullTextSearch, S context) => default!;
    public virtual T Visit<S>(JsonOperator jsonOperator, S context) => default!;
    public virtual T Visit<S>(DoubleAnd doubleAnd, S context) => VisitBinary(doubleAnd);
    public virtual T Visit<S>(Contains contains, S context) => default!;
    public virtual T Visit<S>(ContainedBy containedBy, S context) => default!;
    public virtual T Visit<S>(Matches matches, S context) => default!;
    public virtual T Visit<S>(RegExpMatchOperator regExpMatchOperator, S context) => default!;
    public virtual T Visit<S>(MemberOfExpression memberOfExpression, S context) => default!;

    public virtual T Visit<S>(Column column, S context) => default!;

    public virtual T Visit<S>(AllColumns allColumns, S context) => default!;
    public virtual T Visit<S>(AllTableColumns allTableColumns, S context) => default!;
    public virtual T Visit<S>(Select select, S context) => default!;

    public virtual T Visit<S>(CastExpression castExpression, S context) { castExpression.Expression?.Accept(this); return default!; }
    public virtual T Visit<S>(AnalyticExpression analyticExpression, S context) => default!;
    public virtual T Visit<S>(ExtractExpression extractExpression, S context) => default!;
    public virtual T Visit<S>(IntervalExpression intervalExpression, S context) => default!;

    // JSqlParser 5.0 new expressions
    public virtual T Visit<S>(LambdaExpression lambdaExpression, S context) { lambdaExpression.Expression?.Accept(this); return default!; }
    public virtual T Visit<S>(StructType structType, S context) => default!;
    public virtual T Visit<S>(Operators.Relational.ExcludesExpression excludesExpression, S context) { excludesExpression.LeftExpression?.Accept(this); excludesExpression.RightExpression?.Accept(this); return default!; }
    public virtual T Visit<S>(Operators.Relational.IncludesExpression includesExpression, S context) { includesExpression.LeftExpression?.Accept(this); includesExpression.RightExpression?.Accept(this); return default!; }

    // JSqlParser 5.1 new expressions
    public virtual T Visit<S>(BooleanValue booleanValue, S context) => default!;
    public virtual T Visit<S>(ConnectByPriorOperator connectByPriorOperator, S context) { connectByPriorOperator.Expression?.Accept(this); return default!; }
    public virtual T Visit<S>(HighExpression highExpression, S context) { highExpression.Expression?.Accept(this); return default!; }
    public virtual T Visit<S>(LowExpression lowExpression, S context) { lowExpression.Expression?.Accept(this); return default!; }
    public virtual T Visit<S>(Inverse inverse, S context) { inverse.Expression?.Accept(this); return default!; }
    public virtual T Visit<S>(Operators.Relational.CosineSimilarity cosineSimilarity, S context) => VisitBinary(cosineSimilarity);
    public virtual T Visit<S>(Operators.Relational.Plus plus, S context) => VisitBinary(plus);
    public virtual T Visit<S>(Operators.Relational.PriorTo priorTo, S context) => VisitBinary(priorTo);

    // JSqlParser 5.2 new expressions
    public virtual T Visit<S>(Operators.Relational.IsUnknownExpression isUnknownExpression, S context) { isUnknownExpression.LeftExpression?.Accept(this); return default!; }
    public virtual T Visit<S>(Statement.Select.FunctionAllColumns functionAllColumns, S context) => default!;

    // Collection expressions
    public virtual T Visit<S>(MultiAndExpression multiAndExpression, S context) { foreach (var expr in multiAndExpression.Expressions) expr.Accept(this, context); return default!; }
    public virtual T Visit<S>(ExpressionList expressionList, S context) { foreach (var expr in expressionList.Expressions) expr.Accept(this, context); return default!; }
    public virtual T Visit<S>(RowGetExpression rowGetExpression, S context) { rowGetExpression.Expression?.Accept(this, context); return default!; }
    public virtual T Visit<S>(KeyExpression keyExpression, S context) { keyExpression.Expression?.Accept(this, context); return default!; }

    private T VisitBinary(BinaryExpression binary)
    {
        binary.LeftExpression.Accept(this);
        binary.RightExpression.Accept(this);
        return default!;
    }
}
