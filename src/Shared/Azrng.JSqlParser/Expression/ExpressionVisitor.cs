using Azrng.JSqlParser.Expression.Cnf;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Visitor interface for traversing SQL expression trees.
/// Generic version (JSqlParser 5.0+): returns T, accepts context S.
/// </summary>
public interface ExpressionVisitor<T>
{
    // Value types
    T Visit<S>(NullValue nullValue, S context);
    T Visit<S>(LongValue longValue, S context);
    T Visit<S>(DoubleValue doubleValue, S context);
    T Visit<S>(StringValue stringValue, S context);
    T Visit<S>(DateValue dateValue, S context);
    T Visit<S>(TimeValue timeValue, S context);
    T Visit<S>(TimestampValue timestampValue, S context);
    T Visit<S>(HexValue hexValue, S context);

    // Parameters
    T Visit<S>(JdbcParameter jdbcParameter, S context);
    T Visit<S>(JdbcNamedParameter jdbcNamedParameter, S context);

    // Basic expressions
    T Visit<S>(Parenthesis parenthesis, S context);
    T Visit<S>(SignedExpression signedExpression, S context);
    T Visit<S>(Function function, S context);
    T Visit<S>(CaseExpression caseExpression, S context);
    T Visit<S>(WhenClause whenClause, S context);

    // Arithmetic operators
    T Visit<S>(Addition addition, S context);
    T Visit<S>(Division division, S context);
    T Visit<S>(IntegerDivision division, S context);
    T Visit<S>(Multiplication multiplication, S context);
    T Visit<S>(Subtraction subtraction, S context);
    T Visit<S>(Modulo modulo, S context);
    T Visit<S>(Concat concat, S context);
    T Visit<S>(BitwiseAnd bitwiseAnd, S context);
    T Visit<S>(BitwiseOr bitwiseOr, S context);
    T Visit<S>(BitwiseXor bitwiseXor, S context);
    T Visit<S>(BitwiseLeftShift bitwiseLeftShift, S context);
    T Visit<S>(BitwiseRightShift bitwiseRightShift, S context);

    // Conditional operators
    T Visit<S>(AndExpression andExpression, S context);
    T Visit<S>(OrExpression orExpression, S context);
    T Visit<S>(XorExpression xorExpression, S context);
    T Visit<S>(NotExpression notExpression, S context);

    // Relational operators
    T Visit<S>(EqualsTo equalsTo, S context);
    T Visit<S>(NotEqualsTo notEqualsTo, S context);
    T Visit<S>(GreaterThan greaterThan, S context);
    T Visit<S>(GreaterThanEquals greaterThanEquals, S context);
    T Visit<S>(MinorThan minorThan, S context);
    T Visit<S>(MinorThanEquals minorThanEquals, S context);
    T Visit<S>(Between between, S context);
    T Visit<S>(InExpression inExpression, S context);
    T Visit<S>(IsNullExpression isNullExpression, S context);
    T Visit<S>(IsBooleanExpression isBooleanExpression, S context);
    T Visit<S>(IsDistinctExpression isDistinctExpression, S context);
    T Visit<S>(LikeExpression likeExpression, S context);
    T Visit<S>(SimilarToExpression similarToExpression, S context);
    T Visit<S>(ExistsExpression existsExpression, S context);
    T Visit<S>(FullTextSearch fullTextSearch, S context);
    T Visit<S>(JsonOperator jsonOperator, S context);
    T Visit<S>(DoubleAnd doubleAnd, S context);
    T Visit<S>(Contains contains, S context);
    T Visit<S>(ContainedBy containedBy, S context);
    T Visit<S>(Matches matches, S context);
    T Visit<S>(RegExpMatchOperator regExpMatchOperator, S context);
    T Visit<S>(MemberOfExpression memberOfExpression, S context);

    // Schema types
    T Visit<S>(Column column, S context);

    // Select-related
    T Visit<S>(Azrng.JSqlParser.Statement.Select.AllColumns allColumns, S context);
    T Visit<S>(Azrng.JSqlParser.Statement.Select.AllTableColumns allTableColumns, S context);
    T Visit<S>(Azrng.JSqlParser.Statement.Select.Select select, S context);

    // Advanced expressions
    T Visit<S>(CastExpression castExpression, S context);
    T Visit<S>(AnalyticExpression analyticExpression, S context);
    T Visit<S>(ExtractExpression extractExpression, S context);
    T Visit<S>(IntervalExpression intervalExpression, S context);

    // JSqlParser 5.0 new expressions
    T Visit<S>(LambdaExpression lambdaExpression, S context);
    T Visit<S>(StructType structType, S context);
    T Visit<S>(Operators.Relational.ExcludesExpression excludesExpression, S context);
    T Visit<S>(Operators.Relational.IncludesExpression includesExpression, S context);

    // JSqlParser 5.1 new expressions
    T Visit<S>(BooleanValue booleanValue, S context);
    T Visit<S>(ConnectByPriorOperator connectByPriorOperator, S context);
    T Visit<S>(ConnectByRootOperator connectByRootOperator, S context);
    T Visit<S>(HighExpression highExpression, S context);
    T Visit<S>(LowExpression lowExpression, S context);
    T Visit<S>(Inverse inverse, S context);
    T Visit<S>(RangeExpression rangeExpression, S context);
    T Visit<S>(TimeKeyExpression timeKeyExpression, S context);
    T Visit<S>(TranscodingFunction transcodingFunction, S context);
    T Visit<S>(JsonFunction jsonFunction, S context);
    T Visit<S>(JsonAggregateFunction jsonAggregateFunction, S context);
    T Visit<S>(Operators.Relational.CosineSimilarity cosineSimilarity, S context);
    T Visit<S>(Operators.Relational.GeometryDistance geometryDistance, S context);
    T Visit<S>(Operators.Relational.Plus plus, S context);
    T Visit<S>(Operators.Relational.PriorTo priorTo, S context);

    // JSqlParser 5.2 new expressions
    T Visit<S>(Operators.Relational.IsUnknownExpression isUnknownExpression, S context);
    T Visit<S>(Statement.Select.FunctionAllColumns functionAllColumns, S context);

    // Collection expressions (DIM defaults - iterate children)
    T Visit<S>(MultiAndExpression multiAndExpression, S context)
    {
        foreach (var expr in multiAndExpression.Expressions)
            expr.Accept(this, context);
        return default!;
    }
    T Visit<S>(ExpressionList expressionList, S context)
    {
        foreach (var expr in expressionList.Expressions)
            expr.Accept(this, context);
        return default!;
    }
    T Visit<S>(RowGetExpression rowGetExpression, S context)
    {
        rowGetExpression.Expression?.Accept(this, context);
        return default!;
    }
    T Visit<S>(KeyExpression keyExpression, S context)
    {
        keyExpression.Expression?.Accept(this, context);
        return default!;
    }
    T Visit<S>(DateUnitExpression dateUnitExpression, S context) => default!;
    T Visit<S>(OracleNamedFunctionParameter oracleNamedFunctionParameter, S context)
    {
        oracleNamedFunctionParameter.Expression?.Accept(this, context);
        return default!;
    }
    T Visit<S>(PostgresNamedFunctionParameter postgresNamedFunctionParameter, S context)
    {
        postgresNamedFunctionParameter.Expression?.Accept(this, context);
        return default!;
    }
    T Visit<S>(OracleHint oracleHint, S context) => default!;
    T Visit<S>(KeepExpression keepExpression, S context)
    {
        if (keepExpression.OrderByElements != null)
        {
            foreach (var obe in keepExpression.OrderByElements)
            {
                obe.Expression?.Accept(this, context);
            }
        }
        return default!;
    }
    T Visit<S>(TrimFunction trimFunction, S context)
    {
        trimFunction.Expression?.Accept(this, context);
        trimFunction.FromExpression?.Accept(this, context);
        return default!;
    }
    T Visit<S>(CollateExpression collateExpression, S context)
    {
        collateExpression.LeftExpression?.Accept(this, context);
        return default!;
    }
    T Visit<S>(TimezoneExpression timezoneExpression, S context)
    {
        timezoneExpression.LeftExpression?.Accept(this, context);
        timezoneExpression.TimeZoneExpression?.Accept(this, context);
        return default!;
    }
    T Visit<S>(NextValExpression nextValExpression, S context) => default!;
    T Visit<S>(AnyComparisonExpression anyComparisonExpression, S context) => default!;
    T Visit<S>(ArrayConstructor arrayConstructor, S context)
    {
        if (arrayConstructor.Expressions?.Expressions != null)
        {
            foreach (var expr in arrayConstructor.Expressions.Expressions)
            {
                expr.Accept(this, context);
            }
        }
        return default!;
    }
    T Visit<S>(ArrayExpression arrayExpression, S context)
    {
        arrayExpression.ObjExpression?.Accept(this, context);
        arrayExpression.IndexExpression?.Accept(this, context);
        arrayExpression.StartIndexExpression?.Accept(this, context);
        arrayExpression.StopIndexExpression?.Accept(this, context);
        return default!;
    }
    T Visit<S>(RowConstructor rowConstructor, S context)
    {
        if (rowConstructor.Expressions?.Expressions != null)
        {
            foreach (var expr in rowConstructor.Expressions.Expressions)
            {
                expr.Accept(this, context);
            }
        }
        return default!;
    }

    // Convenience overloads (no context)
    void Visit(NullValue nullValue) => Visit<object?>(nullValue, default);
    void Visit(LongValue longValue) => Visit<object?>(longValue, default);
    void Visit(DoubleValue doubleValue) => Visit<object?>(doubleValue, default);
    void Visit(StringValue stringValue) => Visit<object?>(stringValue, default);
    void Visit(DateValue dateValue) => Visit<object?>(dateValue, default);
    void Visit(TimeValue timeValue) => Visit<object?>(timeValue, default);
    void Visit(TimestampValue timestampValue) => Visit<object?>(timestampValue, default);
    void Visit(HexValue hexValue) => Visit<object?>(hexValue, default);
    void Visit(JdbcParameter jdbcParameter) => Visit<object?>(jdbcParameter, default);
    void Visit(JdbcNamedParameter jdbcNamedParameter) => Visit<object?>(jdbcNamedParameter, default);
    void Visit(Parenthesis parenthesis) => Visit<object?>(parenthesis, default);
    void Visit(SignedExpression signedExpression) => Visit<object?>(signedExpression, default);
    void Visit(Function function) => Visit<object?>(function, default);
    void Visit(CaseExpression caseExpression) => Visit<object?>(caseExpression, default);
    void Visit(WhenClause whenClause) => Visit<object?>(whenClause, default);
    void Visit(Addition addition) => Visit<object?>(addition, default);
    void Visit(Division division) => Visit<object?>(division, default);
    void Visit(IntegerDivision division) => Visit<object?>(division, default);
    void Visit(Multiplication multiplication) => Visit<object?>(multiplication, default);
    void Visit(Subtraction subtraction) => Visit<object?>(subtraction, default);
    void Visit(Modulo modulo) => Visit<object?>(modulo, default);
    void Visit(Concat concat) => Visit<object?>(concat, default);
    void Visit(BitwiseAnd bitwiseAnd) => Visit<object?>(bitwiseAnd, default);
    void Visit(BitwiseOr bitwiseOr) => Visit<object?>(bitwiseOr, default);
    void Visit(BitwiseXor bitwiseXor) => Visit<object?>(bitwiseXor, default);
    void Visit(BitwiseLeftShift bitwiseLeftShift) => Visit<object?>(bitwiseLeftShift, default);
    void Visit(BitwiseRightShift bitwiseRightShift) => Visit<object?>(bitwiseRightShift, default);
    void Visit(AndExpression andExpression) => Visit<object?>(andExpression, default);
    void Visit(OrExpression orExpression) => Visit<object?>(orExpression, default);
    void Visit(XorExpression xorExpression) => Visit<object?>(xorExpression, default);
    void Visit(NotExpression notExpression) => Visit<object?>(notExpression, default);
    void Visit(EqualsTo equalsTo) => Visit<object?>(equalsTo, default);
    void Visit(NotEqualsTo notEqualsTo) => Visit<object?>(notEqualsTo, default);
    void Visit(GreaterThan greaterThan) => Visit<object?>(greaterThan, default);
    void Visit(GreaterThanEquals greaterThanEquals) => Visit<object?>(greaterThanEquals, default);
    void Visit(MinorThan minorThan) => Visit<object?>(minorThan, default);
    void Visit(MinorThanEquals minorThanEquals) => Visit<object?>(minorThanEquals, default);
    void Visit(Between between) => Visit<object?>(between, default);
    void Visit(InExpression inExpression) => Visit<object?>(inExpression, default);
    void Visit(IsNullExpression isNullExpression) => Visit<object?>(isNullExpression, default);
    void Visit(IsBooleanExpression isBooleanExpression) => Visit<object?>(isBooleanExpression, default);
    void Visit(IsDistinctExpression isDistinctExpression) => Visit<object?>(isDistinctExpression, default);
    void Visit(LikeExpression likeExpression) => Visit<object?>(likeExpression, default);
    void Visit(SimilarToExpression similarToExpression) => Visit<object?>(similarToExpression, default);
    void Visit(ExistsExpression existsExpression) => Visit<object?>(existsExpression, default);
    void Visit(FullTextSearch fullTextSearch) => Visit<object?>(fullTextSearch, default);
    void Visit(JsonOperator jsonOperator) => Visit<object?>(jsonOperator, default);
    void Visit(DoubleAnd doubleAnd) => Visit<object?>(doubleAnd, default);
    void Visit(Contains contains) => Visit<object?>(contains, default);
    void Visit(ContainedBy containedBy) => Visit<object?>(containedBy, default);
    void Visit(Matches matches) => Visit<object?>(matches, default);
    void Visit(RegExpMatchOperator regExpMatchOperator) => Visit<object?>(regExpMatchOperator, default);
    void Visit(MemberOfExpression memberOfExpression) => Visit<object?>(memberOfExpression, default);
    void Visit(Column column) => Visit<object?>(column, default);
    void Visit(Azrng.JSqlParser.Statement.Select.AllColumns allColumns) => Visit<object?>(allColumns, default);
    void Visit(Azrng.JSqlParser.Statement.Select.AllTableColumns allTableColumns) => Visit<object?>(allTableColumns, default);
    void Visit(Azrng.JSqlParser.Statement.Select.Select select) => Visit<object?>(select, default);
    void Visit(CastExpression castExpression) => Visit<object?>(castExpression, default);
    void Visit(AnalyticExpression analyticExpression) => Visit<object?>(analyticExpression, default);
    void Visit(ExtractExpression extractExpression) => Visit<object?>(extractExpression, default);
    void Visit(IntervalExpression intervalExpression) => Visit<object?>(intervalExpression, default);
    void Visit(LambdaExpression lambdaExpression) => Visit<object?>(lambdaExpression, default);
    void Visit(StructType structType) => Visit<object?>(structType, default);
    void Visit(Operators.Relational.ExcludesExpression excludesExpression) => Visit<object?>(excludesExpression, default);
    void Visit(Operators.Relational.IncludesExpression includesExpression) => Visit<object?>(includesExpression, default);
    void Visit(BooleanValue booleanValue) => Visit<object?>(booleanValue, default);
    void Visit(ConnectByPriorOperator connectByPriorOperator) => Visit<object?>(connectByPriorOperator, default);
    void Visit(ConnectByRootOperator connectByRootOperator) => Visit<object?>(connectByRootOperator, default);
    void Visit(HighExpression highExpression) => Visit<object?>(highExpression, default);
    void Visit(LowExpression lowExpression) => Visit<object?>(lowExpression, default);
    void Visit(Inverse inverse) => Visit<object?>(inverse, default);
    void Visit(RangeExpression rangeExpression) => Visit<object?>(rangeExpression, default);
    void Visit(TimeKeyExpression timeKeyExpression) => Visit<object?>(timeKeyExpression, default);
    void Visit(TranscodingFunction transcodingFunction) => Visit<object?>(transcodingFunction, default);
    void Visit(JsonFunction jsonFunction) => Visit<object?>(jsonFunction, default);
    void Visit(JsonAggregateFunction jsonAggregateFunction) => Visit<object?>(jsonAggregateFunction, default);
    void Visit(Operators.Relational.CosineSimilarity cosineSimilarity) => Visit<object?>(cosineSimilarity, default);
    void Visit(Operators.Relational.GeometryDistance geometryDistance) => Visit<object?>(geometryDistance, default);
    void Visit(Operators.Relational.Plus plus) => Visit<object?>(plus, default);
    void Visit(Operators.Relational.PriorTo priorTo) => Visit<object?>(priorTo, default);
    void Visit(Operators.Relational.IsUnknownExpression isUnknownExpression) => Visit<object?>(isUnknownExpression, default);
    void Visit(Statement.Select.FunctionAllColumns functionAllColumns) => Visit<object?>(functionAllColumns, default);
    void Visit(MultiAndExpression multiAndExpression) => Visit<object?>(multiAndExpression, default);
    void Visit(ExpressionList expressionList) => Visit<object?>(expressionList, default);
    void Visit(RowGetExpression rowGetExpression) => Visit<object?>(rowGetExpression, default);
    void Visit(KeyExpression keyExpression) => Visit<object?>(keyExpression, default);
    void Visit(DateUnitExpression dateUnitExpression) => Visit<object?>(dateUnitExpression, default);
    void Visit(OracleNamedFunctionParameter oracleNamedFunctionParameter) => Visit<object?>(oracleNamedFunctionParameter, default);
    void Visit(PostgresNamedFunctionParameter postgresNamedFunctionParameter) => Visit<object?>(postgresNamedFunctionParameter, default);
    void Visit(OracleHint oracleHint) => Visit<object?>(oracleHint, default);
    void Visit(KeepExpression keepExpression) => Visit<object?>(keepExpression, default);
    void Visit(TrimFunction trimFunction) => Visit<object?>(trimFunction, default);
    void Visit(CollateExpression collateExpression) => Visit<object?>(collateExpression, default);
    void Visit(TimezoneExpression timezoneExpression) => Visit<object?>(timezoneExpression, default);
    void Visit(NextValExpression nextValExpression) => Visit<object?>(nextValExpression, default);
    void Visit(AnyComparisonExpression anyComparisonExpression) => Visit<object?>(anyComparisonExpression, default);
    void Visit(ArrayConstructor arrayConstructor) => Visit<object?>(arrayConstructor, default);
    void Visit(ArrayExpression arrayExpression) => Visit<object?>(arrayExpression, default);
    void Visit(RowConstructor rowConstructor) => Visit<object?>(rowConstructor, default);
}
