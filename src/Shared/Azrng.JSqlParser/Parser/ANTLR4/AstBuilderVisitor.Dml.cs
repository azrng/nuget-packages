using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Globalization;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Show;
using Azrng.JSqlParser.Statement.Delete;
using Azrng.JSqlParser.Statement.Insert;
using Azrng.JSqlParser.Statement.Lock;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Update;
using Azrng.JSqlParser.Statement.CreateTable;
using Azrng.JSqlParser.Statement.Create.Policy;
using Azrng.JSqlParser.Statement.Create.Schema;
using Azrng.JSqlParser.Statement.Create.Sequence;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.Analyze;
using Azrng.JSqlParser.Statement.Comment;
using Azrng.JSqlParser.Statement.Execute;
using Azrng.JSqlParser.Statement.Create.Synonym;
using Azrng.JSqlParser.Statement.Create.Function;
using Azrng.JSqlParser.Statement.Create.Procedure;
using Azrng.JSqlParser.Statement.Drop;
using Azrng.JSqlParser.Statement.Truncate;
using Azrng.JSqlParser.Statement.CreateView;
using Azrng.JSqlParser.Statement.CreateIndex;
using Azrng.JSqlParser.Statement.Merge;
using Azrng.JSqlParser.Statement.Piped;

namespace Azrng.JSqlParser.Parser.ANTLR4;

/// <summary>
/// AstBuilderVisitor 的 Dml 分部：INSERT / UPDATE / DELETE / MERGE / UPSERT / RETURNING 等数据操作。
/// </summary>
public partial class AstBuilderVisitor
{
    public override object VisitInsertStatement(JSqlParserGrammar.InsertStatementContext context)
    {
        var insert = new Insert();
        insert.Table = (Table)Visit(context.table());

        // MySQL INSERT 修饰符：LOW_PRIORITY / DELAYED / HIGH_PRIORITY / IGNORE
        if (context.LOW_PRIORITY() != null) insert.ModifierPriority = InsertModifierPriority.LowPriority;
        else if (context.DELAYED() != null) insert.ModifierPriority = InsertModifierPriority.Delayed;
        else if (context.HIGH_PRIORITY() != null) insert.ModifierPriority = InsertModifierPriority.HighPriority;
        if (context.IGNORE() != null) insert.ModifierIgnore = true;

        if (context.identifierList() != null)
        {
            insert.Columns = new List<Column>();
            foreach (var id in context.identifierList().identifier())
            {
                insert.Columns.Add(new Column { ColumnName = id.GetText() });
            }
        }

        // MSSQL OUTPUT 子句（透传原始文本保 round-trip）
        if (context.outputClause() is { } outputCtx)
            insert.OutputClause = GetOriginalText(outputCtx);

        if (context.selectStatement() != null)
        {
            insert.Select = (Select)Visit(context.selectStatement());
        }
        else if (context.SET() != null)
        {
            // MySQL INSERT INTO t SET col=val, col2=val2（对齐 #1314）
            insert.UseValues = false;
            insert.UseSet = true;
            insert.SetUpdateSets = new List<UpdateSet>();
            foreach (var assignment in context.assignmentItem())
            {
                var updateSet = new UpdateSet
                {
                    Columns = new List<Column>(),
                    Values = new List<Expression.IExpression>()
                };
                foreach (var target in assignment.assignmentTarget())
                    updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                updateSet.Values.Add((Expression.IExpression)Visit(assignment.expression()));
                insert.SetUpdateSets.Add(updateSet);
            }
        }
        else if (context.valuesList() != null)
        {
            insert.UseValues = true;
            insert.ValuesItems = new List<ExpressionList>();
            foreach (var itemCtx in context.valuesList().valuesItem())
            {
                var exprList = new ExpressionList
                {
                    Expressions = new List<Expression.IExpression>()
                };
                foreach (var exprCtx in itemCtx.expression())
                {
                    exprList.Expressions.Add((Expression.IExpression)Visit(exprCtx));
                }
                insert.ValuesItems.Add(exprList);
            }
        }

        if (context.onDuplicateKey() != null)
        {
            var dupCtx = context.onDuplicateKey();
            if (dupCtx.NOTHING() != null)
            {
                insert.DuplicateUpdateNothing = true;
            }
            else
            {
                insert.DuplicateUpdateSets = new List<UpdateSet>();
                foreach (var assignment in dupCtx.assignmentItem())
                {
                    var updateSet = new UpdateSet();
                    updateSet.Columns = new List<Column>();
                    foreach (var target in assignment.assignmentTarget())
                    {
                        updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                    }
                    updateSet.Values = new List<Expression.IExpression>();
                    updateSet.Values.Add((Expression.IExpression)Visit(assignment.expression()));
                    insert.DuplicateUpdateSets.Add(updateSet);
                }
                // MySQL 8.0.20+ ON DUPLICATE KEY UPDATE ... WHERE
                if (dupCtx.whereClause() != null)
                {
                    insert.DuplicateUpdateWhereExpression =
                        (Expression.IExpression)Visit(dupCtx.whereClause().expression());
                }
            }
        }

        if (context.onConflictClause() != null)
        {
            var onConflictCtx = context.onConflictClause();
            if (onConflictCtx.insertConflictTarget() != null)
            {
                insert.ConflictTarget = (InsertConflictTarget)Visit(onConflictCtx.insertConflictTarget());
            }
            insert.ConflictAction = (InsertConflictAction)Visit(onConflictCtx.insertConflictAction());
        }

        if (context.returningClause() != null)
        {
            insert.Returning = (ReturningClause)Visit(context.returningClause());
        }

        return insert;
    }

    // PostgreSQL ON CONFLICT 冲突目标：(col1, col2) [WHERE pred] 或 ON CONSTRAINT name

    public override object VisitInsertConflictTarget(JSqlParserGrammar.InsertConflictTargetContext context)
    {
        var target = new InsertConflictTarget();
        var identifiers = context.identifier();

        if (context.CONSTRAINT() != null && identifiers.Length > 0)
        {
            // ON CONSTRAINT name 形式：取第一个（也是唯一的）identifier
            target.ConstraintName = identifiers[0].GetText();
        }
        else
        {
            // (col1, col2, ...) [WHERE pred] 索引列形式
            foreach (var id in identifiers)
            {
                target.IndexColumnNames.Add(id.GetText());
            }
            if (context.whereClause() != null)
            {
                target.WhereExpression = (Expression.IExpression)Visit(context.whereClause().expression());
            }
        }

        return target;
    }

    // PostgreSQL ON CONFLICT 冲突动作：DO NOTHING | DO UPDATE SET ... [WHERE ...]

    public override object VisitInsertConflictAction(JSqlParserGrammar.InsertConflictActionContext context)
    {
        var action = new InsertConflictAction();

        if (context.NOTHING() != null)
        {
            action.ConflictActionType = ConflictActionType.DoNothing;
        }
        else if (context.UPDATE() != null)
        {
            action.ConflictActionType = ConflictActionType.DoUpdate;
            action.UpdateSets = new List<UpdateSet>();
            foreach (var assignment in context.assignmentItem())
            {
                var updateSet = new UpdateSet();
                updateSet.Columns = new List<Column>();
                foreach (var target in assignment.assignmentTarget())
                {
                    updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                }
                updateSet.Values = new List<Expression.IExpression>();
                updateSet.Values.Add((Expression.IExpression)Visit(assignment.expression()));
                action.UpdateSets.Add(updateSet);
            }
            if (context.whereClause() != null)
            {
                action.WhereExpression = (Expression.IExpression)Visit(context.whereClause().expression());
            }
        }

        return action;
    }

    public override object VisitMultiInsertStatement(JSqlParserGrammar.MultiInsertStatementContext context)
    {
        var multiInsert = new MultiInsert
        {
            // INSERT FIRST 命中即停；INSERT ALL 评估全部 WHEN
            IsFirst = context.FIRST() != null
        };

        foreach (var branchCtx in context.multiInsertBranch())
        {
            multiInsert.Branches.Add((MultiInsertBranch)Visit(branchCtx));
        }

        if (context.selectStatement() != null)
        {
            multiInsert.Select = (Select)Visit(context.selectStatement());
        }

        return multiInsert;
    }

    public override object VisitMultiInsertBranch(JSqlParserGrammar.MultiInsertBranchContext context)
    {
        var branch = new MultiInsertBranch();

        // WHEN expression THEN / ELSE / 无条件 INTO
        if (context.WHEN() != null && context.expression() != null)
        {
            branch.WhenCondition = (Expression.IExpression)Visit(context.expression());
        }
        else if (context.ELSE() != null)
        {
            branch.IsElse = true;
        }

        // 收集分支下的所有 INTO 目标子句（一个分支可含多个目标）
        foreach (var clauseCtx in context.multiInsertClause())
        {
            branch.Clauses.Add((MultiInsertClause)Visit(clauseCtx));
        }

        return branch;
    }

    public override object VisitMultiInsertClause(JSqlParserGrammar.MultiInsertClauseContext context)
    {
        var clause = new MultiInsertClause
        {
            Table = (Table)Visit(context.table())
        };

        if (context.identifierList() != null)
        {
            clause.Columns = new List<Column>();
            foreach (var id in context.identifierList().identifier())
            {
                clause.Columns.Add(new Column { ColumnName = id.GetText() });
            }
        }

        // VALUES valuesList | selectStatement
        if (context.valuesList() != null)
        {
            clause.UseValues = true;
            clause.ValuesItems = new List<ExpressionList>();
            foreach (var itemCtx in context.valuesList().valuesItem())
            {
                var exprList = new ExpressionList
                {
                    Expressions = new List<Expression.IExpression>()
                };
                foreach (var exprCtx in itemCtx.expression())
                {
                    exprList.Expressions.Add((Expression.IExpression)Visit(exprCtx));
                }
                clause.ValuesItems.Add(exprList);
            }
        }
        else if (context.selectStatement() != null)
        {
            clause.UseValues = false;
            clause.Select = (Select)Visit(context.selectStatement());
        }

        return clause;
    }

    public override object VisitUpdateStatement(JSqlParserGrammar.UpdateStatementContext context)
    {
        var update = new Update();
        // MySQL 修饰符：LOW_PRIORITY / IGNORE
        if (context.LOW_PRIORITY() != null) update.ModifierLowPriority = true;
        if (context.IGNORE() != null) update.ModifierIgnore = true;
        update.Table = (Table)Visit(context.table());

        var joinClauses = context.joinClause();
        if (joinClauses.Length > 0)
        {
            update.Joins = new List<Join>();
            foreach (var joinCtx in joinClauses)
            {
                update.Joins.Add((Join)Visit(joinCtx));
            }
        }

        update.UpdateSets = new List<UpdateSet>();
        foreach (var assignment in context.assignmentItem())
        {
            var updateSet = new UpdateSet();
            updateSet.Columns = new List<Column>();
            foreach (var target in assignment.assignmentTarget())
            {
                updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
            }
            updateSet.Values = new List<Expression.IExpression>();
            updateSet.Values.Add((Expression.IExpression)Visit(assignment.expression()));
            update.UpdateSets.Add(updateSet);
        }

        if (context.whereClause() != null)
        {
            update.Where = (Expression.IExpression)Visit(context.whereClause().expression());
        }

        if (context.returningClause() != null)
        {
            update.Returning = (ReturningClause)Visit(context.returningClause());
        }

        return update;
    }

    public override object VisitDeleteStatement(JSqlParserGrammar.DeleteStatementContext context)
    {
        var delete = new Delete();
        // MySQL 修饰符：LOW_PRIORITY / QUICK / IGNORE
        if (context.LOW_PRIORITY() != null) delete.ModifierLowPriority = true;
        if (context.QUICK() != null) delete.ModifierQuick = true;
        if (context.IGNORE() != null) delete.ModifierIgnore = true;
        delete.Table = (Table)Visit(context.table());

        // DELETE ... USING fromItem (COMMA fromItem)*
        // PostgreSQL/SQL Server 风格的 DELETE USING 子句，允许引用附加表参与 WHERE 连接
        var usingItems = context.fromItem();
        if (context.USING() != null && usingItems.Length > 0)
        {
            delete.UsingItems = new List<IFromItem>();
            foreach (var fromCtx in usingItems)
            {
                delete.UsingItems.Add((IFromItem)Visit(fromCtx.tableOrSubquery()));
            }
        }

        if (context.whereClause() != null)
        {
            delete.Where = (Expression.IExpression)Visit(context.whereClause().expression());
        }

        if (context.returningClause() != null)
        {
            delete.Returning = (ReturningClause)Visit(context.returningClause());
        }

        return delete;
    }

    public override object VisitUpsertStatement(JSqlParserGrammar.UpsertStatementContext context)
    {
        var upsert = new Statement.Insert.UpsertStatement();
        // 类型判定
        if (context.UPSERT() != null) upsert.UpsertType = Statement.Insert.UpsertType.Upsert;
        else if (context.REPLACE() != null && context.INSERT() == null) upsert.UpsertType = Statement.Insert.UpsertType.Replace;
        else upsert.UpsertType = Statement.Insert.UpsertType.InsertOrReplace;

        if (context.INTO() != null) upsert.UseInto = true;
        if (context.table() is { } tblCtx) upsert.Table = (Table)Visit(tblCtx);

        // 列
        if (context.identifierList() != null)
        {
            upsert.Columns = new List<Column>();
            foreach (var id in context.identifierList().identifier())
                upsert.Columns.Add(new Column { ColumnName = id.GetText() });
        }

        // SET / SELECT / VALUES 三选一
        if (context.SET() != null)
        {
            upsert.SetUpdateSets = new List<UpdateSet>();
            foreach (var assignment in context.assignmentItem())
            {
                var updateSet = new UpdateSet
                {
                    Columns = new List<Column>(),
                    Values = new List<Expression.IExpression>()
                };
                foreach (var target in assignment.assignmentTarget())
                    updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                updateSet.Values.Add((Expression.IExpression)Visit(assignment.expression()));
                upsert.SetUpdateSets.Add(updateSet);
            }
        }
        else if (context.selectStatement() != null)
        {
            upsert.Select = (Select)Visit(context.selectStatement());
        }
        else if (context.valuesList() != null)
        {
            upsert.UseValues = true;
            upsert.ValuesItems = new List<ExpressionList>();
            foreach (var itemCtx in context.valuesList().valuesItem())
            {
                var exprList = new ExpressionList { Expressions = new List<Expression.IExpression>() };
                foreach (var exprCtx in itemCtx.expression())
                    exprList.Expressions.Add((Expression.IExpression)Visit(exprCtx));
                upsert.ValuesItems.Add(exprList);
            }
        }

        // ON DUPLICATE KEY UPDATE
        if (context.onDuplicateKey() is { } dupCtx)
        {
            if (dupCtx.NOTHING() != null)
            {
                upsert.DuplicateUpdateNothing = true;
            }
            else
            {
                upsert.DuplicateUpdateSets = new List<UpdateSet>();
                foreach (var assignment in dupCtx.assignmentItem())
                {
                    var updateSet = new UpdateSet
                    {
                        Columns = new List<Column>(),
                        Values = new List<Expression.IExpression>()
                    };
                    foreach (var target in assignment.assignmentTarget())
                        updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                    updateSet.Values.Add((Expression.IExpression)Visit(assignment.expression()));
                    upsert.DuplicateUpdateSets.Add(updateSet);
                }
            }
        }

        return upsert;
    }

    public override object VisitMergeStatement(JSqlParserGrammar.MergeStatementContext context)
    {
        var merge = new Statement.Merge.Merge();
        merge.Table = (Table)Visit(context.table());

        // MERGE INTO t alias USING ...（目标表别名）
        if (context.alias() != null)
        {
            merge.Alias = new Alias(context.alias().identifier().GetText(),
                context.alias().AS() != null);
        }

        // USING 源（表/子查询），grammar 保证必现
        if (context.fromItem() != null)
        {
            merge.SourceTable = (IFromItem)Visit(context.fromItem());
        }

        merge.OnCondition = (Expression.IExpression)Visit(context.expression());

        foreach (var whenCtx in context.mergeWhenClause())
        {
            // WHEN [NOT] MATCHED (AND expression)? THEN ...
            // mergeWhenClause 内 AND 后的 expression（单个，grammar 每分支至多一个）
            Expression.IExpression? whenAndCondition = null;
            if (whenCtx.AND() != null && whenCtx.expression() != null)
            {
                whenAndCondition = (Expression.IExpression)Visit(whenCtx.expression());
            }

            if (whenCtx.UPDATE() != null)
            {
                var op = new Statement.Merge.MergeUpdate();
                if (whenCtx.NOT() != null) op.Not = true;
                op.Condition = whenAndCondition;
                foreach (var assignment in whenCtx.assignmentItem())
                {
                    var updateSet = new UpdateSet();
                    updateSet.Columns = new List<Column>();
                    foreach (var target in assignment.assignmentTarget())
                    {
                        updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                    }
                    updateSet.Values = new List<Expression.IExpression>();
                    updateSet.Values.Add((Expression.IExpression)Visit(assignment.expression()));
                    op.UpdateSets.Add(updateSet);
                }
                merge.Operations.Add(op);
            }
            else if (whenCtx.DELETE() != null)
            {
                var op = new Statement.Merge.MergeDelete();
                if (whenCtx.NOT() != null) op.Not = true;
                op.Condition = whenAndCondition;
                merge.Operations.Add(op);
            }
            else if (whenCtx.INSERT() != null)
            {
                var op = new Statement.Merge.MergeInsert();
                if (whenCtx.NOT() != null) op.Not = true;
                op.Condition = whenAndCondition;
                if (whenCtx.identifierList() != null)
                {
                    op.Columns = new List<Column>();
                    foreach (var id in whenCtx.identifierList().identifier())
                    {
                        op.Columns.Add(new Column { ColumnName = id.GetText() });
                    }
                }
                // VALUES valuesItem（grammar 保证 INSERT 分支必现）
                if (whenCtx.valuesItem() != null)
                {
                    op.Values = new List<Expression.IExpression>();
                    foreach (var exprCtx in whenCtx.valuesItem().expression())
                    {
                        op.Values.Add((Expression.IExpression)Visit(exprCtx));
                    }
                }
                merge.Operations.Add(op);
            }
        }

        return merge;
    }

    public override object VisitReturningClause(JSqlParserGrammar.ReturningClauseContext context)
    {
        var keyword = context.RETURNING() != null
            ? ReturningClause.Keyword.RETURNING
            : ReturningClause.Keyword.RETURN;

        List<ReturningOutputAlias>? outputAliases = null;
        if (context.WITH() != null)
        {
            outputAliases = new List<ReturningOutputAlias>();
            foreach (var aliasCtx in context.returningOutputAlias())
            {
                outputAliases.Add((ReturningOutputAlias)Visit(aliasCtx));
            }
        }

        var selectItems = new List<SelectItem>();
        foreach (var itemCtx in context.selectColumnList().selectItem())
        {
            selectItems.Add((SelectItem)Visit(itemCtx));
        }

        var clause = new ReturningClause(keyword, selectItems, outputAliases);

        // RETURNING ... INTO target1, target2（Oracle PL/SQL 变量绑定）
        if (context.INTO() != null)
        {
            clause.DataItems = new List<string>();
            foreach (var tableCtx in context.table())
            {
                clause.DataItems.Add(((Table)Visit(tableCtx)).GetFullyQualifiedName());
            }
        }

        // PostgreSQL 18 OLD/NEW 引用归一化：将 old.price / new.* 的限定符
        // 从 Table 迁移到 ReturningReferenceType + ReturningQualifier。
        // 若指定了 WITH 别名，按别名映射；否则默认 "old"->OLD, "new"->NEW。
        NormalizeReturningReferences(clause);

        return clause;
    }

    public override object VisitReturningOutputAlias(JSqlParserGrammar.ReturningOutputAliasContext context)
    {
        var ids = context.identifier();
        var refType = ReturningReferenceTypeExtensions.From(ids[0].GetText())
            ?? throw new InvalidOperationException(
                $"Expected OLD or NEW but found: {ids[0].GetText()}");
        return new ReturningOutputAlias(refType, ids[1].GetText());
    }

    /// <summary>
    /// 将 RETURNING 列表中的 old./new. 限定符（或 WITH 别名定义的别名前缀）
    /// 从 Table 迁移到 ReturningReferenceType + ReturningQualifier，并清空 Table。
    /// </summary>
    private static void NormalizeReturningReferences(ReturningClause clause)
    {
        // 构建限定符 -> 引用类型 的映射
        var qualifierMap = new Dictionary<string, ReturningReferenceType>(StringComparer.OrdinalIgnoreCase);
        if (clause.OutputAliases != null && clause.OutputAliases.Count > 0)
        {
            foreach (var alias in clause.OutputAliases)
            {
                if (alias.Alias != null)
                {
                    qualifierMap[alias.Alias] = alias.ReferenceType;
                }
            }
        }
        else
        {
            qualifierMap["old"] = ReturningReferenceType.Old;
            qualifierMap["new"] = ReturningReferenceType.New;
        }

        foreach (var item in clause.SelectItems)
        {
            if (item.Expression is Column col)
            {
                NormalizeColumnReference(col, qualifierMap);
            }
            else if (item.Expression is AllTableColumns allCols)
            {
                NormalizeAllTableColumnsReference(allCols, qualifierMap);
            }
        }
    }

    private static void NormalizeColumnReference(Column col, Dictionary<string, ReturningReferenceType> qualifierMap)
    {
        var table = col.Table;
        // 仅当限定符是简单表名（无 schema/database）时才识别为 OLD/NEW 引用
        if (table == null || table.SchemaName != null || table.Database != null) return;

        var qualifier = table.Name;
        if (qualifier == null || qualifier.Contains('@')) return;

        if (qualifierMap.TryGetValue(qualifier, out var refType))
        {
            col.ReturningReferenceType = refType;
            col.ReturningQualifier = qualifier;
            col.Table = null;
        }
    }

    private static void NormalizeAllTableColumnsReference(AllTableColumns allCols, Dictionary<string, ReturningReferenceType> qualifierMap)
    {
        var table = allCols.Table;
        if (table == null || table.SchemaName != null || table.Database != null) return;

        var qualifier = table.Name;
        if (qualifier == null || qualifier.Contains('@')) return;

        if (qualifierMap.TryGetValue(qualifier, out var refType))
        {
            allCols.ReturningReferenceType = refType;
            allCols.ReturningQualifier = qualifier;
            allCols.Table = null!;
        }
    }
}
