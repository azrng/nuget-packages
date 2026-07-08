using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Util;

/// <summary>
/// Finds all table names referenced in a SQL statement.
/// Implements both ExpressionVisitor and StatementVisitor to traverse the AST.
/// </summary>
public class TablesNamesFinder : ExpressionVisitor<object?>, Statement.StatementVisitor<object?>
{
    private readonly HashSet<string> _tables = new();

    public HashSet<string> GetTables(Statement.Statement statement)
    {
        _tables.Clear();
        statement.Accept(this);
        return _tables;
    }

    private void AddTable(Table? table)
    {
        if (table != null)
        {
            var name = table.Name;
            if (!string.IsNullOrEmpty(name))
                _tables.Add(name);
        }
    }

    // ExpressionVisitor<object?> implementation
    public object? Visit<S>(NullValue nullValue, S context) => null;
    public object? Visit<S>(LongValue longValue, S context) => null;
    public object? Visit<S>(DoubleValue doubleValue, S context) => null;
    public object? Visit<S>(StringValue stringValue, S context) => null;
    public object? Visit<S>(DateValue dateValue, S context) => null;
    public object? Visit<S>(TimeValue timeValue, S context) => null;
    public object? Visit<S>(TimestampValue timestampValue, S context) => null;
    public object? Visit<S>(HexValue hexValue, S context) => null;
    public object? Visit<S>(JdbcParameter jdbcParameter, S context) => null;
    public object? Visit<S>(JdbcNamedParameter jdbcNamedParameter, S context) => null;

    public object? Visit<S>(Parenthesis parenthesis, S context)
    {
        parenthesis.Expression.Accept(this);
        return null;
    }

    public object? Visit<S>(SignedExpression signedExpression, S context)
    {
        signedExpression.Expression.Accept(this);
        return null;
    }

    public object? Visit<S>(Function function, S context)
    {
        function.Parameters?.Accept(this);
        return null;
    }

    public object? Visit<S>(CaseExpression caseExpression, S context)
    {
        caseExpression.SwitchExpression?.Accept(this);
        caseExpression.ElseExpression?.Accept(this);
        if (caseExpression.WhenClauses != null)
        {
            foreach (var when in caseExpression.WhenClauses)
                when.Accept(this, context);
        }
        return null;
    }

    public object? Visit<S>(WhenClause whenClause, S context)
    {
        whenClause.WhenExpression.Accept(this);
        whenClause.ThenExpression.Accept(this);
        return null;
    }

    // Arithmetic operators
    public object? Visit<S>(Addition addition, S context) => VisitBinary(addition);
    public object? Visit<S>(Division division, S context) => VisitBinary(division);
    public object? Visit<S>(IntegerDivision division, S context) => VisitBinary(division);
    public object? Visit<S>(Multiplication multiplication, S context) => VisitBinary(multiplication);
    public object? Visit<S>(Subtraction subtraction, S context) => VisitBinary(subtraction);
    public object? Visit<S>(Modulo modulo, S context) => VisitBinary(modulo);
    public object? Visit<S>(Concat concat, S context) => VisitBinary(concat);
    public object? Visit<S>(BitwiseAnd bitwiseAnd, S context) => VisitBinary(bitwiseAnd);
    public object? Visit<S>(BitwiseOr bitwiseOr, S context) => VisitBinary(bitwiseOr);
    public object? Visit<S>(BitwiseXor bitwiseXor, S context) => VisitBinary(bitwiseXor);
    public object? Visit<S>(BitwiseLeftShift bitwiseLeftShift, S context) => VisitBinary(bitwiseLeftShift);
    public object? Visit<S>(BitwiseRightShift bitwiseRightShift, S context) => VisitBinary(bitwiseRightShift);

    // Conditional operators
    public object? Visit<S>(AndExpression andExpression, S context) => VisitBinary(andExpression);
    public object? Visit<S>(OrExpression orExpression, S context) => VisitBinary(orExpression);
    public object? Visit<S>(XorExpression xorExpression, S context) => VisitBinary(xorExpression);
    public object? Visit<S>(NotExpression notExpression, S context)
    {
        notExpression.Expression.Accept(this);
        return null;
    }

    // Relational operators
    public object? Visit<S>(EqualsTo equalsTo, S context) => VisitBinary(equalsTo);
    public object? Visit<S>(NotEqualsTo notEqualsTo, S context) => VisitBinary(notEqualsTo);
    public object? Visit<S>(GreaterThan greaterThan, S context) => VisitBinary(greaterThan);
    public object? Visit<S>(GreaterThanEquals greaterThanEquals, S context) => VisitBinary(greaterThanEquals);
    public object? Visit<S>(MinorThan minorThan, S context) => VisitBinary(minorThan);
    public object? Visit<S>(MinorThanEquals minorThanEquals, S context) => VisitBinary(minorThanEquals);

    public object? Visit<S>(Between between, S context)
    {
        between.LeftExpression.Accept(this);
        between.BetweenExpressionStart.Accept(this);
        between.BetweenExpressionEnd.Accept(this);
        return null;
    }

    public object? Visit<S>(InExpression inExpression, S context)
    {
        inExpression.LeftExpression.Accept(this);
        if (inExpression.RightExpression != null)
            inExpression.RightExpression.Accept(this);
        return null;
    }

    public object? Visit<S>(IsNullExpression isNullExpression, S context)
    {
        isNullExpression.LeftExpression.Accept(this);
        return null;
    }

    public object? Visit<S>(IsBooleanExpression isBooleanExpression, S context)
    {
        isBooleanExpression.LeftExpression.Accept(this);
        return null;
    }

    public object? Visit<S>(IsDistinctExpression isDistinctExpression, S context)
    {
        isDistinctExpression.LeftExpression.Accept(this);
        isDistinctExpression.RightExpression.Accept(this);
        return null;
    }

    public object? Visit<S>(LikeExpression likeExpression, S context) => VisitBinary(likeExpression);
    public object? Visit<S>(SimilarToExpression similarToExpression, S context) => VisitBinary(similarToExpression);

    public object? Visit<S>(ExistsExpression existsExpression, S context)
    {
        if (existsExpression.RightExpression != null)
            existsExpression.RightExpression.Accept(this);
        return null;
    }

    public object? Visit<S>(FullTextSearch fullTextSearch, S context) => null;
    public object? Visit<S>(JsonOperator jsonOperator, S context) => VisitBinary(jsonOperator);
    public object? Visit<S>(DoubleAnd doubleAnd, S context) => VisitBinary(doubleAnd);
    public object? Visit<S>(Contains contains, S context) => VisitBinary(contains);
    public object? Visit<S>(ContainedBy containedBy, S context) => VisitBinary(containedBy);
    public object? Visit<S>(Matches matches, S context) => VisitBinary(matches);
    public object? Visit<S>(RegExpMatchOperator regExpMatchOperator, S context) => VisitBinary(regExpMatchOperator);
    public object? Visit<S>(MemberOfExpression memberOfExpression, S context)
    {
        memberOfExpression.LeftExpression.Accept(this);
        memberOfExpression.RightExpression.Accept(this);
        return null;
    }

    public object? Visit<S>(OverlapsCondition overlapsCondition, S context)
    {
        overlapsCondition.LeftExpression.Accept(this, context);
        overlapsCondition.RightExpression.Accept(this, context);
        return null;
    }

    public object? Visit<S>(Column column, S context)
    {
        if (column.Table != null)
            AddTable(column.Table);
        return null;
    }

    public object? Visit<S>(AllColumns allColumns, S context) => null;
    public object? Visit<S>(AllTableColumns allTableColumns, S context)
    {
        AddTable(allTableColumns.Table);
        return null;
    }

    public object? Visit<S>(CastExpression castExpression, S context)
    {
        castExpression.Expression.Accept(this);
        return null;
    }

    public object? Visit<S>(AnalyticExpression analyticExpression, S context)
    {
        analyticExpression.Expression?.Accept(this);
        analyticExpression.Offset?.Accept(this);
        analyticExpression.DefaultValue?.Accept(this);
        return null;
    }

    public object? Visit<S>(ExtractExpression extractExpression, S context)
    {
        extractExpression.Expression.Accept(this);
        return null;
    }

    public object? Visit<S>(IntervalExpression intervalExpression, S context)
    {
        intervalExpression.Expression?.Accept(this);
        return null;
    }

    public object? Visit<S>(LambdaExpression lambdaExpression, S context)
    {
        lambdaExpression.Expression?.Accept(this);
        return null;
    }

    public object? Visit<S>(StructType structType, S context) => null;
    public object? Visit<S>(ExcludesExpression excludesExpression, S context)
    {
        excludesExpression.LeftExpression?.Accept(this);
        excludesExpression.RightExpression?.Accept(this);
        return null;
    }

    public object? Visit<S>(IncludesExpression includesExpression, S context)
    {
        includesExpression.LeftExpression?.Accept(this);
        includesExpression.RightExpression?.Accept(this);
        return null;
    }

    // JSqlParser 5.1 new expressions
    public object? Visit<S>(BooleanValue booleanValue, S context) => null;
    public object? Visit<S>(DateTimeLiteralExpression dateTimeLiteralExpression, S context) => null;
    public object? Visit<S>(ConnectByPriorOperator connectByPriorOperator, S context) { connectByPriorOperator.Expression?.Accept(this); return null; }
    public object? Visit<S>(ConnectByRootOperator connectByRootOperator, S context) { connectByRootOperator.Expression?.Accept(this); return null; }
    public object? Visit<S>(HighExpression highExpression, S context) { highExpression.Expression?.Accept(this); return null; }
    public object? Visit<S>(LowExpression lowExpression, S context) { lowExpression.Expression?.Accept(this); return null; }
    public object? Visit<S>(Inverse inverse, S context) { inverse.Expression?.Accept(this); return null; }
    public object? Visit<S>(CosineSimilarity cosineSimilarity, S context) => VisitBinary(cosineSimilarity);
    public object? Visit<S>(GeometryDistance geometryDistance, S context) => VisitBinary(geometryDistance);
    public object? Visit<S>(RangeExpression rangeExpression, S context)
    {
        rangeExpression.StartExpression?.Accept(this);
        rangeExpression.EndExpression?.Accept(this);
        return null;
    }
    public object? Visit<S>(TimeKeyExpression timeKeyExpression, S context) => null;
    public object? Visit<S>(TranscodingFunction transcodingFunction, S context)
    {
        transcodingFunction.Expression?.Accept(this);
        return null;
    }
    public object? Visit<S>(JsonFunction jsonFunction, S context)
    {
        foreach (var kvp in jsonFunction.KeyValuePairs)
        {
            if (kvp.Key is Expression.Expression ke) ke.Accept(this);
            if (kvp.Value is Expression.Expression ve) ve.Accept(this);
        }
        foreach (var expr in jsonFunction.Expressions)
        {
            expr.Expression?.Accept(this);
        }
        foreach (var expr in jsonFunction.PassingExpressions)
        {
            expr.Accept(this);
        }
        jsonFunction.InputExpression?.Expression?.Accept(this);
        jsonFunction.JsonPathExpression?.Accept(this);
        return null;
    }
    public object? Visit<S>(JsonAggregateFunction jsonAggregateFunction, S context)
    {
        jsonAggregateFunction.Value?.Accept(this);
        jsonAggregateFunction.AggregateExpression?.Accept(this);
        jsonAggregateFunction.FilterExpression?.Accept(this);
        return null;
    }
    public object? Visit<S>(Plus plus, S context) => VisitBinary(plus);
    public object? Visit<S>(PriorTo priorTo, S context) => VisitBinary(priorTo);

    // JSqlParser 5.2 new expressions
    public object? Visit<S>(IsUnknownExpression isUnknownExpression, S context) { isUnknownExpression.LeftExpression?.Accept(this); return null; }
    public object? Visit<S>(Statement.Select.FunctionAllColumns functionAllColumns, S context) => null;

    object? ExpressionVisitor<object?>.Visit<S>(Azrng.JSqlParser.Statement.Select.Select select, S context)
    {
        // Delegate to StatementVisitor logic to traverse subquery contents
        ((Statement.StatementVisitor<object?>)this).Visit(select, context);
        return null;
    }

    private object? VisitBinary(BinaryExpression binaryExpression)
    {
        binaryExpression.LeftExpression.Accept(this);
        binaryExpression.RightExpression.Accept(this);
        return null;
    }

    // StatementVisitor<object?> implementation
    public object? Visit<S>(Statement.Statements stmts, S context)
    {
        foreach (var stmt in stmts.StatementList)
            stmt.Accept(this);
        return null;
    }

    public object? Visit<S>(Select select, S context)
    {
        if (select.WithItemsList != null)
        {
            foreach (var withItem in select.WithItemsList)
                VisitWithItem(withItem);
        }

        if (select is PlainSelect plainSelect)
            VisitPlainSelect(plainSelect);
        else if (select is SetOperationList setOpList)
            VisitSetOperationList(setOpList);
        else if (select is Statement.Piped.FromQuery fromQuery)
            VisitFromQuery(fromQuery);
        return null;
    }

    private void VisitWithItem(WithItem withItem)
    {
        if (withItem.Select != null)
            ((Statement.StatementVisitor<object?>)this).Visit(withItem.Select, (object?)null);
    }

    private void VisitPlainSelect(PlainSelect plainSelect)
    {
        VisitFromItem(plainSelect.FromItem);

        if (plainSelect.Joins != null)
        {
            foreach (var join in plainSelect.Joins)
            {
                VisitFromItem(join.RightItem);
                foreach (var onExpr in join.OnExpressions) onExpr.Accept(this);
            }
        }

        plainSelect.Where?.Accept(this);

        if (plainSelect.SelectItems != null)
        {
            foreach (var item in plainSelect.SelectItems)
                item.Expression.Accept(this);
        }

        plainSelect.Having?.Accept(this);
    }

    private void VisitSetOperationList(SetOperationList setOpList)
    {
        foreach (var s in setOpList.Selects)
            ((Statement.StatementVisitor<object?>)this).Visit(s, (object?)null);
    }

    private void VisitFromQuery(Statement.Piped.FromQuery fromQuery)
    {
        VisitFromItem(fromQuery.FromItem);

        if (fromQuery.Joins != null)
        {
            foreach (var join in fromQuery.Joins)
            {
                VisitFromItem(join.RightItem);
                foreach (var onExpr in join.OnExpressions) onExpr.Accept(this);
            }
        }

        foreach (var op in fromQuery.PipeOperators)
        {
            if (op is Statement.Piped.JoinPipeOperator joinOp && joinOp.Join.RightItem is Table pipeJoinTable)
                AddTable(pipeJoinTable);
        }
    }

    private void VisitFromItem(FromItem? fromItem)
    {
        if (fromItem is Table table)
        {
            AddTable(table);
        }
        else if (fromItem is ParenthesedSelect parenthesedSelect)
        {
            ((Statement.StatementVisitor<object?>)this).Visit(parenthesedSelect.Select, (object?)null);
        }
    }

    public object? Visit<S>(Statement.Insert.Insert insert, S context)
    {
        AddTable(insert.Table);
        if (insert.Select != null)
            ((Statement.StatementVisitor<object?>)this).Visit(insert.Select, (object?)null);
        return null;
    }

    public object? Visit<S>(Statement.Insert.MultiInsert multiInsert, S context)
    {
        foreach (var branch in multiInsert.Branches)
        {
            branch.WhenCondition?.Accept(this);
            foreach (var clause in branch.Clauses)
            {
                AddTable(clause.Table);
                if (clause.Select != null)
                    ((Statement.StatementVisitor<object?>)this).Visit(clause.Select, (object?)null);
            }
        }
        if (multiInsert.Select != null)
            ((Statement.StatementVisitor<object?>)this).Visit(multiInsert.Select, (object?)null);
        return null;
    }

    public object? Visit<S>(Statement.Update.Update update, S context)
    {
        AddTable(update.Table);
        update.Where?.Accept(this);
        return null;
    }

    public object? Visit<S>(Statement.Delete.Delete delete, S context)
    {
        AddTable(delete.Table);
        if (delete.UsingItems != null)
        {
            foreach (var fromItem in delete.UsingItems)
            {
                VisitFromItem(fromItem);
            }
        }
        delete.Where?.Accept(this);
        return null;
    }

    public object? Visit<S>(Statement.Merge.Merge merge, S context)
    {
        AddTable(merge.Table);
        merge.OnCondition?.Accept(this);
        return null;
    }

    public object? Visit<S>(Statement.CreateTable.CreateTable createTable, S context)
    {
        AddTable(createTable.Table);
        return null;
    }

    public object? Visit<S>(Statement.CreateView.CreateView createView, S context)
    {
        AddTable(createView.View);
        if (createView.Select != null)
            ((Statement.StatementVisitor<object?>)this).Visit(createView.Select, (object?)null);
        return null;
    }

    public object? Visit<S>(Statement.CreateIndex.CreateIndex createIndex, S context)
    {
        AddTable(createIndex.Table);
        return null;
    }

    public object? Visit<S>(Statement.Alter.Alter alter, S context)
    {
        AddTable(alter.Table);
        return null;
    }

    public object? Visit<S>(Statement.Alter.RenameTableStatement rename, S context)
    {
        foreach (var pair in rename.TableNames)
        {
            AddTable(pair.Key);
            AddTable(pair.Value);
        }
        return null;
    }

    public object? Visit<S>(Statement.Analyze.Analyze analyze, S context) { AddTable(analyze.Table); return null; }
    public object? Visit<S>(Statement.Comment.Comment comment, S context) { if (comment.Table != null) AddTable(comment.Table); return null; }
    public object? Visit<S>(Statement.Execute.Execute execute, S context) => null;
    public object? Visit<S>(Statement.PurgeStatement purge, S context) { if (purge.Table != null) AddTable(purge.Table); return null; }
    public object? Visit<S>(Statement.Alter.AlterView alterView, S context) { AddTable(alterView.View); return null; }
    public object? Visit<S>(Statement.Alter.AlterSession alterSession, S context) => null;
    public object? Visit<S>(Statement.Alter.AlterSystemStatement alterSystem, S context) => null;
    public object? Visit<S>(Statement.Create.Synonym.CreateSynonym createSynonym, S context) => null;
    public object? Visit<S>(Statement.Block block, S context) { if (block.Statements?.StatementList != null) foreach (var s in block.Statements.StatementList) s.Accept(this); return null; }
    public object? Visit<S>(Statement.DeclareStatement declare, S context) => null;
    public object? Visit<S>(Statement.IfElseStatement ifElse, S context) { ifElse.IfStatement?.Accept(this); ifElse.ElseStatement?.Accept(this); return null; }
    public object? Visit<S>(Statement.Create.Function.CreateFunction createFunction, S context) => null;
    public object? Visit<S>(Statement.Create.Procedure.CreateProcedure createProcedure, S context) => null;

    public object? Visit<S>(Statement.Drop.Drop drop, S context)
    {
        AddTable(drop.Name);
        return null;
    }

    public object? Visit<S>(Statement.Truncate.Truncate truncate, S context)
    {
        AddTable(truncate.Table);
        return null;
    }

    public object? Visit<S>(Statement.CommitStatement commitStatement, S context) => null;
    public object? Visit<S>(Statement.RollbackStatement rollbackStatement, S context) => null;
    public object? Visit<S>(Statement.SavepointStatement savepointStatement, S context) => null;
    public object? Visit<S>(Statement.UseStatement use, S context) => null;
    public object? Visit<S>(Statement.SetStatement set, S context) => null;
    public object? Visit<S>(Statement.ResetStatement reset, S context) => null;
    public object? Visit<S>(Statement.ShowStatement show, S context) => null;
    public object? Visit<S>(Statement.Show.ShowColumnsStatement showColumns, S context) { if (showColumns.Table != null) AddTable(showColumns.Table); return null; }
    public object? Visit<S>(Statement.Show.ShowIndexStatement showIndex, S context) { if (showIndex.Table != null) AddTable(showIndex.Table); return null; }
    public object? Visit<S>(Statement.Show.ShowTablesStatement showTables, S context) => null;
    public object? Visit<S>(Statement.DescribeStatement describe, S context) => null;
    public object? Visit<S>(Statement.ExplainStatement explain, S context) => null;
    public object? Visit<S>(Statement.GrantStatement grant, S context)
    {
        AddTable(grant.Table);
        return null;
    }
    public object? Visit<S>(Statement.UnsupportedStatement unsupportedStatement, S context) => null;

    // JSqlParser 5.1 - Parenthesized DML for CTEs
    public object? Visit<S>(Statement.Select.ParenthesedInsert parenthesedInsert, S context)
    {
        if (parenthesedInsert.Insert != null)
            ((Statement.StatementVisitor<object?>)this).Visit(parenthesedInsert.Insert, (object?)null);
        return null;
    }
    public object? Visit<S>(Statement.Select.ParenthesedUpdate parenthesedUpdate, S context) { ((Statement.StatementVisitor<object?>)this).Visit(parenthesedUpdate.Update, (object?)null); return null; }
    public object? Visit<S>(Statement.Select.ParenthesedDelete parenthesedDelete, S context) { ((Statement.StatementVisitor<object?>)this).Visit(parenthesedDelete.Delete, (object?)null); return null; }

    // JSqlParser 5.4
    public object? Visit<S>(Statement.SessionStatement sessionStatement, S context) => null;

    // JSqlParser 5.4+ - LOCK TABLE
    public object? Visit<S>(Statement.Lock.LockStatement lockStatement, S context)
    {
        AddTable(lockStatement.Table);
        return null;
    }

    // JSqlParser 5.4+ - CREATE POLICY (PostgreSQL RLS)
    public object? Visit<S>(Statement.Create.Policy.CreatePolicy createPolicy, S context)
    {
        AddTable(createPolicy.Table);
        return null;
    }

    // JSqlParser 5.4+ - CREATE SEQUENCE
    public object? Visit<S>(Statement.Create.Sequence.CreateSequence createSequence, S context) => null;

    public object? Visit<S>(Statement.Create.Schema.CreateSchema createSchema, S context) => null;
}
