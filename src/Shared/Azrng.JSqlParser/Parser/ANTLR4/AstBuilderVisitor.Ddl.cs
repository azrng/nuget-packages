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
/// AstBuilderVisitor 的 Ddl 分部：CREATE / ALTER / DROP / RENAME / 约束 / 列定义 / 分区等模式定义。
/// </summary>
public partial class AstBuilderVisitor
{
    public override object VisitCreateTable(JSqlParserGrammar.CreateTableContext context)
    {
        var create = new CreateTable();
        // createTable 产生式含多个 table 引用（主表 / LIKE 表 / spannerInterleaveIn 表），第一个为主表
        var tables = context.table();
        create.Table = tables.Length > 0 ? (Table)Visit(tables[0]) : null;

        // CREATE 子句选项
        create.OrReplace = context.OR() != null;
        create.Unlogged = context.UNLOGGED() != null;
        if (context.IF() != null) create.IfNotExists = true;
        if (context.createOption() is { Length: > 0 } opts)
            create.CreateOptions = opts.Select(o => o.GetText()).ToList();

        // 括号内定义：simpleColumnNames（仅列名）或 createTableDefinition 列表
        if (context.simpleColumnNames() != null)
        {
            create.Columns = context.simpleColumnNames().identifier().Select(i => i.GetText()).ToList();
        }
        else
        {
            create.ColumnDefinitions = new List<ColumnDefinition>();
            create.Constraints = new List<Constraint>();
            foreach (var defCtx in context.createTableDefinition())
            {
                if (defCtx.columnDefinition() != null)
                    create.ColumnDefinitions.Add((ColumnDefinition)Visit(defCtx.columnDefinition()));
                else if (defCtx.tableConstraint() != null)
                    create.Constraints.Add((Constraint)Visit(defCtx.tableConstraint()));
            }
        }

        // 表级选项（ENGINE/CHARSET/PARTITION BY/ORDER BY/SAMPLE BY 等），createParameter 透传为字符串
        create.TableOptions = CollectCreateParameters(context.createParameter());

        // Oracle ROW MOVEMENT
        if (context.rowMovementClause() is { } rmCtx)
        {
            var mode = rmCtx.ENABLE() != null ? RowMovementMode.Enable : RowMovementMode.Disable;
            create.RowMovement = new RowMovement { Mode = mode };
        }

        // AS SELECT (CTAS)
        if (context.selectStatement() != null)
            create.Select = (Statement.Select.Select)Visit(context.selectStatement());

        // LIKE table
        if (context.LIKE() != null && tables.Length > 1)
            create.LikeTable = (Table)Visit(tables[1]);

        // Spanner INTERLEAVE IN PARENT
        if (context.spannerInterleaveIn() is { } interleaveCtx)
        {
            create.InterleaveIn = new SpannerInterleaveIn
            {
                Table = interleaveCtx.table() is { } parentTable ? (Table)Visit(parentTable) : null,
                OnDelete = interleaveCtx.ON() != null && interleaveCtx.DELETE() != null
                    ? new ReferentialAction
                    {
                        Type = ReferentialActionType.Delete,
                        Action = interleaveCtx.CASCADE() != null ? ReferentialActionMode.Cascade : ReferentialActionMode.NoAction
                    }
                    : null
            };
        }

        return create;
    }

    public override object VisitColumnDefinition(JSqlParserGrammar.ColumnDefinitionContext context)
    {
        var colDef = new ColumnDefinition();
        colDef.ColumnName = context.identifier().GetText();
        colDef.ColDataType = BuildColDataType(context.colDataType());
        // 列规格透传为字符串（NOT NULL / DEFAULT expr / MATERIALIZED / COMMENT '...' 等），对齐上游 columnSpecs
        // 结构化 columnConstraint + 兜底 createParameter 均收集原始文本，保 round-trip
        var specs = new List<string>();
        foreach (var cc in context.columnConstraint())
            specs.Add(GetOriginalText(cc));
        specs.AddRange(CollectCreateParameters(context.createParameter()));
        colDef.ColumnSpecs = specs;
        return colDef;
    }

    /// <summary>
    /// 从 colDataType 产生式构造结构化 <see cref="ColDataType"/>（类型名/参数/数组维度），对齐上游。
    /// 支持三路：ARRAY&lt;T&gt;（扁平化整体存 DataType，对齐上游 DataType() ARRAY 分支）、
    /// STRUCT(fld type, ...)（DataType="STRUCT"，字段进 ArgumentsStringList，对齐上游 ColDataType() STRUCT 分支）、
    /// 普通 dataType + 可选 PostgreSQL [] 数组维度。
    /// </summary>
    private static ColDataType BuildColDataType(JSqlParserGrammar.ColDataTypeContext? ctx)
    {
        var result = new ColDataType();
        if (ctx == null) return result;

        // ARRAY<T>：整体扁平化为 "ARRAY<...>"（含嵌套），对齐上游 setDataType("ARRAY<" + inner + ">")
        if (ctx.ARRAY() != null)
        {
            result.DataType = ctx.GetText();
            return result;
        }

        // STRUCT(fld type, ...)：DataType="STRUCT"，字段列表进 ArgumentsStringList
        if (ctx.STRUCT() != null)
        {
            result.DataType = "STRUCT";
            result.ArgumentsStringList = ctx.structColField()
                .Select(f => $"{f.identifier().GetText()} {BuildColDataType(f.colDataType())}")
                .ToList();
            return result;
        }

        // 普通类型：dataType 原始文本（保留 CHARACTER VARYING / set('a','b') 等空格）+ 可选 TIME ZONE 后缀
        // 数组维度 int[5] 不进 DataType，由 ArrayData 单独表示（避免 ToString 重复输出）
        var dtCtx = ctx.dataType();
        if (dtCtx != null)
        {
            var typeText = GetOriginalText(dtCtx);
            if (ctx.timeZoneSuffix() is { } tz)
                typeText += " " + GetOriginalText(tz);
            result.DataType = typeText;
        }
        // 数组维度 int[5]：填充 ArrayData（带尺寸），对齐上游 arrayData
        var dims = ctx.arrayDimension();
        if (dims is { Length: > 0 })
        {
            result.ArrayData = dims.Select(d =>
            {
                var n = d.LONG_VALUE();
                return n != null ? ParseInt(n.GetText()) : (int?)null;
            }).ToList();
        }
        return result;
    }

    /// <summary>
    /// 收集 createParameter 产生式的原始文本为字符串列表（保 round-trip），对齐上游 CreateParameter + tableOptionsStrings/columnSpecs。
    /// 使用 InputStream 区间获取原始字符流文本，保留 token 间的空格/等号风格。
    /// </summary>
    private static List<string> CollectCreateParameters(JSqlParserGrammar.CreateParameterContext[]? parameters)
    {
        var result = new List<string>();
        if (parameters == null) return result;
        foreach (var p in parameters)
            AddOriginalText(result, p);
        return result;
    }

    public override object VisitTableConstraint(JSqlParserGrammar.TableConstraintContext context)
    {
        // CONSTRAINT name 中的 name 是第一个 identifier（若有 CONSTRAINT 关键字）
        var identifiers = context.identifier();
        string? name = null;
        if (context.CONSTRAINT() != null && identifiers.Length > 0)
            name = identifiers[0].GetText();

        // EXCLUDE WHERE (expr) — PostgreSQL
        if (context.EXCLUDE() != null)
        {
            var exclude = new ExcludeConstraint { Name = name };
            if (context.expression() != null)
                exclude.Expression = (Expression.IExpression)Visit(context.expression());
            return exclude;
        }

        // CHECK (expr) — 持有表达式
        if (context.CHECK() != null)
        {
            var check = new CheckConstraint { Name = name };
            if (context.expression() != null)
                check.Expression = (Expression.IExpression)Visit(context.expression());
            return check;
        }

        // FOREIGN KEY — 持有引用表/引用列/ON DELETE/UPDATE 动作
        if (context.FOREIGN() != null)
        {
            var identifierLists = context.identifierList();
            var localList = identifierLists.Length > 0 ? identifierLists[0] : null;
            var fk = new ForeignKeyIndex
            {
                Name = name,
                Type = "FOREIGN KEY",
                Columns = ExtractIdentifierList(localList)
            };
            // REFERENCES table (cols)
            var refTable = context.table();
            if (refTable != null)
                fk.ReferencedTable = (Table)Visit(refTable);
            if (identifierLists.Length > 1)
                fk.ReferencedColumnNames = ExtractIdentifierList(identifierLists[1]);
            // ON DELETE / ON UPDATE 动作
            var (onDelete, onUpdate) = ExtractReferentialActions(context.ON(), context.DELETE(), context.UPDATE(), context.referentialAction());
            fk.OnDelete = onDelete;
            fk.OnUpdate = onUpdate;
            return fk;
        }

        // 其余类型（PRIMARY KEY / UNIQUE / KEY|INDEX）沿用扁平 Constraint
        var constraint = new Constraint { Name = name };
        var identifierLists2 = context.identifierList();
        JSqlParserGrammar.IdentifierListContext? firstList = identifierLists2.Length > 0 ? identifierLists2[0] : null;

        if (context.PRIMARY() != null)
        {
            constraint.Type = "PRIMARY KEY";
            constraint.Columns = ExtractIdentifierList(firstList);
            // SQL Server CLUSTERED / NONCLUSTERED 后缀（对齐 #1589）
            constraint.ClusterKind = context.clusterKind()?.GetText();
            FillUsingIndex(constraint, context);
            CollectIndexOptions(constraint, context);
        }
        else if (context.UNIQUE() != null && context.KEY() == null && context.INDEX() == null
                 && context.indexColumnList() == null)
        {
            constraint.Type = "UNIQUE";
            constraint.Columns = ExtractIdentifierList(firstList);
            // SQL Server CLUSTERED / NONCLUSTERED 后缀（对齐 #1589）
            constraint.ClusterKind = context.clusterKind()?.GetText();
            FillUsingIndex(constraint, context);
            CollectIndexOptions(constraint, context);
        }
        else if (context.UNIQUE() != null && context.KEY() == null && context.INDEX() == null
                 && context.indexColumnList() != null)
        {
            // MySQL：UNIQUE [idx_name] [USING ...] (cols [ASC|DESC]) [index_option ...] —— 无 KEY/INDEX 关键字，
            // 直接 UNIQUE 后跟索引名（#538）。MySQL 8 允许 USING BTREE 出现在列前或列后。
            // 注意：列前 USING 后的 method 名（BTREE/HASH）也是 identifier，会一起进 identifiers 数组，
            // 需按 token 位置区分"索引名"与"index method"。
            constraint.Type = "UNIQUE";
            constraint.ClusterKind = context.clusterKind()?.GetText();
            // 索引名：列前第一个 identifier（在 OPENING_PAREN 之前、且不在 USING 之后）
            var openingParen = context.OPENING_PAREN();
            int parenStartIdx = openingParen != null && openingParen.Length > 0
                ? openingParen[0].Symbol.TokenIndex : int.MaxValue;
            string? indexName = null;
            var preUsingOptions = new List<string>();
            // 遍历 children 找列前 USING->identifier 配对（identifier 是 IdentifierContext），
            // 跳过这些 identifier 不当索引名
            var usingMethodTokenIndices = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < context.ChildCount - 1; i++)
            {
                if (context.GetChild(i) is Antlr4.Runtime.Tree.ITerminalNode term
                    && term.Symbol.Type == JSqlParserGrammar.USING
                    && term.Symbol.TokenIndex < parenStartIdx
                    && context.GetChild(i + 1) is JSqlParserGrammar.IdentifierContext methodCtx)
                {
                    preUsingOptions.Add($"USING {methodCtx.GetText()}");
                    usingMethodTokenIndices.Add(methodCtx.Start.TokenIndex);
                }
            }
            // 索引名 = 列前首个 identifier（非 USING 后的 method）
            foreach (var id in identifiers)
            {
                if (id.Start.TokenIndex < parenStartIdx
                    && !usingMethodTokenIndices.Contains(id.Start.TokenIndex))
                {
                    indexName = id.GetText();
                    break;
                }
            }
            // CONSTRAINT c UNIQUE idx (cols)：identifiers[0]=CONSTRAINT 名（已在方法开头入 Name）
            // 单独 UNIQUE idx (cols)：identifiers[0]=索引名
            // 双名场景（CONSTRAINT + 索引名）：跳过 CONSTRAINT 名
            if (context.CONSTRAINT() != null && indexName != null && name != null && indexName == name)
            {
                // identifiers[0] 是 CONSTRAINT 名，找下一个非 USING method 的 identifier 作索引名
                bool skippedFirst = false;
                foreach (var id in identifiers)
                {
                    if (id.Start.TokenIndex < parenStartIdx
                        && !usingMethodTokenIndices.Contains(id.Start.TokenIndex))
                    {
                        if (!skippedFirst) { skippedFirst = true; continue; }
                        indexName = id.GetText();
                        break;
                    }
                }
            }
            constraint.IndexName = indexName;
            var (colParams, colNames) = ExtractIndexColumnList(context.indexColumnList());
            constraint.IndexColumnParams = colParams;
            constraint.Columns = colNames;
            CollectIndexOptions(constraint, context);
            // 列前 USING 合并到 IndexOptions 前部（输出顺序对齐 MySQL：UNIQUE idx USING BTREE (cols) opts）
            if (preUsingOptions.Count > 0)
            {
                var existing = constraint.IndexOptions ?? new List<string>();
                constraint.IndexOptions = preUsingOptions.Concat(existing).ToList();
            }
        }
        else if (context.KEY() != null || context.INDEX() != null)
        {
            // [UNIQUE | FULLTEXT | SPATIAL] (KEY|INDEX) [name] (cols [ASC|DESC]) — MySQL 索引定义
            var prefix = context.UNIQUE() != null ? "UNIQUE"
                : context.FULLTEXT() != null ? "FULLTEXT"
                : context.SPATIAL() != null ? "SPATIAL" : "";
            var kw = context.INDEX() != null ? "INDEX" : "KEY";
            constraint.Type = string.IsNullOrEmpty(prefix) ? kw : $"{prefix} {kw}";
            // CONSTRAINT c UNIQUE KEY idx (cols)：identifiers[0]=CONSTRAINT 名 c，identifiers[1]=索引名 idx
            //   —— 双名场景，分别入 Name / IndexName（对齐 #1570）
            // UNIQUE KEY idx (cols)：identifiers[0]=索引名 idx —— 单名场景，入 Name 保持兼容
            //   （不设 IndexName，ToString 走单名分支，输出与历史一致）
            if (context.CONSTRAINT() != null && identifiers.Length > 1)
            {
                // Name 已在方法开头由 CONSTRAINT 分支设置（identifiers[0]），此处补索引名
                constraint.IndexName = identifiers[1].GetText();
            }
            else
            {
                int nameIdx = context.CONSTRAINT() != null ? 1 : 0;
                if (identifiers.Length > nameIdx)
                    constraint.Name = identifiers[nameIdx].GetText();
            }
            var (colParams, colNames) = ExtractIndexColumnList(context.indexColumnList());
            constraint.IndexColumnParams = colParams;
            constraint.Columns = colNames;
            CollectIndexOptions(constraint, context);
        }
        return constraint;
    }

    /// <summary>
    /// 从 ON DELETE/UPDATE + referentialAction 节点提取引用动作，返回 (OnDelete, OnUpdate)。
    /// </summary>
    private static (ReferentialAction? onDelete, ReferentialAction? onUpdate) ExtractReferentialActions(
        Antlr4.Runtime.Tree.ITerminalNode[]? onNodes,
        Antlr4.Runtime.Tree.ITerminalNode[]? deleteNodes,
        Antlr4.Runtime.Tree.ITerminalNode[]? updateNodes,
        JSqlParserGrammar.ReferentialActionContext[]? actionContexts)
    {
        ReferentialAction? onDelete = null;
        ReferentialAction? onUpdate = null;
        if (onNodes == null || onNodes.Length == 0) return (onDelete, onUpdate);

        // ON 节点按顺序与 DELETE/UPDATE 配对
        int actionIdx = 0;
        var delCount = deleteNodes?.Length ?? 0;
        var updCount = updateNodes?.Length ?? 0;
        int delUsed = 0, updUsed = 0;
        for (int i = 0; i < onNodes.Length; i++)
        {
            ReferentialActionType type;
            // 判断当前 ON 后跟 DELETE 还是 UPDATE
            if (delUsed < delCount && updUsed < updCount)
            {
                // 两者都还有：按源码顺序判断（ON 节点后紧跟的 terminal）
                // 简化：DELETE 优先匹配（实际 SQL 中 ON DELETE 通常在 ON UPDATE 前）
                type = ReferentialActionType.Delete;
                delUsed++;
            }
            else if (delUsed < delCount)
            {
                type = ReferentialActionType.Delete;
                delUsed++;
            }
            else
            {
                type = ReferentialActionType.Update;
                updUsed++;
            }
            var action = actionIdx < (actionContexts?.Length ?? 0)
                ? ParseReferentialActionMode(actionContexts![actionIdx])
                : ReferentialActionMode.NoAction;
            actionIdx++;
            var ra = new ReferentialAction { Type = type, Action = action };
            if (type == ReferentialActionType.Delete) onDelete = ra;
            else onUpdate = ra;
        }
        return (onDelete, onUpdate);
    }

    /// <summary>解析 referentialAction 产生式为动作模式枚举。</summary>
    private static ReferentialActionMode ParseReferentialActionMode(JSqlParserGrammar.ReferentialActionContext ctx)
    {
        if (ctx.CASCADE() != null) return ReferentialActionMode.Cascade;
        if (ctx.SET() != null && ctx.NULL() != null) return ReferentialActionMode.SetNull;
        if (ctx.SET() != null && ctx.DEFAULT() != null) return ReferentialActionMode.SetDefault;
        if (ctx.RESTRICT() != null) return ReferentialActionMode.Restrict;
        return ReferentialActionMode.NoAction; // NO ACTION
    }

    /// <summary>
    /// 解析约束的 USING INDEX [name] 子句（Oracle/DB2，commit c7b3bdbd）。
    /// </summary>
    private static void FillUsingIndex(Constraint constraint, JSqlParserGrammar.TableConstraintContext context)
    {
        var usingCtx = context.usingIndexClause();
        if (usingCtx == null) return;
        constraint.HasUsingIndex = true;
        var nameId = usingCtx.identifier();
        if (nameId != null)
        {
            constraint.UsingIndex = nameId.GetText();
        }
    }

    /// <summary>
    /// 收集 MySQL 索引尾选项（USING BTREE/HASH、COMMENT '...'、KEY_BLOCK_SIZE n、VISIBLE/INVISIBLE 等），
    /// 用 InputStream 区间取原始文本保 round-trip，对齐上游 IndexOption/idxSpec。
    /// </summary>
    private static void CollectIndexOptions(Constraint constraint, JSqlParserGrammar.TableConstraintContext context)
    {
        var opts = context.indexOption();
        if (opts is { Length: > 0 })
            constraint.IndexOptions = opts.Select(GetOriginalText).ToList();
    }

    private static List<string> ExtractIdentifierList(JSqlParserGrammar.IdentifierListContext? list)
    {
        var result = new List<string>();
        if (list == null) return result;
        foreach (var id in list.identifier())
        {
            result.Add(id.GetText());
        }
        return result;
    }

    /// <summary>
    /// 解析 MySQL 索引列列表（含 ASC/DESC 排序方向，支持表达式索引 (expr)）。
    /// 返回 (含方向的列参数如 "col ASC" 或 "(expr) DESC", 纯列名/表达式文本列表)。
    /// </summary>
    private static (List<string> colParams, List<string> colNames) ExtractIndexColumnList(
        JSqlParserGrammar.IndexColumnListContext? list)
    {
        var colParams = new List<string>();
        var colNames = new List<string>();
        if (list == null) return (colParams, colNames);
        foreach (var col in list.indexColumn())
        {
            // 普通列名或表达式索引 (expr)：identifier 为 null 时取表达式原始文本并补括号
            string name;
            if (col.identifier() != null)
                name = col.identifier().GetText();
            else if (col.expression() != null)
                name = $"({GetOriginalText(col.expression())})";
            else
                name = GetOriginalText(col);
            colNames.Add(name);
            if (col.ASC() != null)
                colParams.Add($"{name} ASC");
            else if (col.DESC() != null)
                colParams.Add($"{name} DESC");
            else
                colParams.Add(name);
        }
        return (colParams, colNames);
    }

    public override object VisitAlterStatement(JSqlParserGrammar.AlterStatementContext context)
    {
        var alter = new Alter();
        alter.Table = (Table)Visit(context.table());

        foreach (var opCtx in context.alterOperation())
        {
            alter.AlterExpressions.Add(BuildAlterExpression(opCtx));
        }
        return alter;
    }

    public override object VisitRenameTableStatement(JSqlParserGrammar.RenameTableStatementContext context)
    {
        var stmt = new RenameTableStatement
        {
            UsingTableKeyword = context.TABLE() != null,
            UsingIfExistsKeyword = context.IF() != null && context.EXISTS() != null
        };

        // WAIT/NOWAIT 指令
        if (context.WAIT() != null && context.LONG_VALUE() != null)
            stmt.WaitDirective = $"WAIT {context.LONG_VALUE().GetText()}";
        else if (context.NOWAIT() != null)
            stmt.WaitDirective = "NOWAIT";

        // 首对 old TO new（grammar 里 table 的索引：[0]=old, [1]=new, [2..]=后续对的 old/new）
        var tables = context.table();
        stmt.AddTableNames((Table)Visit(tables[0]), (Table)Visit(tables[1]));

        // 后续多对（grammar: COMMA table TO table，每对占 2 个 table 节点）
        for (var i = 2; i + 1 < tables.Length; i += 2)
        {
            stmt.AddTableNames((Table)Visit(tables[i]), (Table)Visit(tables[i + 1]));
        }

        return stmt;
    }

    public override object VisitAnalyzeStatement(JSqlParserGrammar.AnalyzeStatementContext context)
    {
        return new Analyze { Table = (Table)Visit(context.table()) };
    }

    public override object VisitCommentStatement(JSqlParserGrammar.CommentStatementContext context)
    {
        var comment = new Comment();
        // grammar: COMMENT ON (TABLE table | COLUMN identifier | VIEW table)
        if (context.VIEW() != null && context.table() is { } viewCtx)
            comment.View = (Table)Visit(viewCtx);
        else if (context.table() is { } tableCtx)
            comment.Table = (Table)Visit(tableCtx);
        else if (context.columnRef() is { } colCtx)
            comment.Column = new Column { ColumnName = colCtx.GetText() };
        comment.CommentText = new StringValue(context.S_CHAR_LITERAL().GetText());
        return comment;
    }

    public override object VisitExecuteStatement(JSqlParserGrammar.ExecuteStatementContext context)
    {
        var execType = context.CALL() != null ? ExecType.CALL
            : context.EXEC() != null ? ExecType.EXEC : ExecType.EXECUTE;
        var exec = new Execute { ExecType = execType, Name = context.identifier().GetText() };
        if (context.expressionList() != null)
        {
            var list = (ExpressionList)Visit(context.expressionList());
            exec.ExprList = list;
        }
        return exec;
    }

    public override object VisitPurgeStatement(JSqlParserGrammar.PurgeStatementContext context)
    {
        var purge = new PurgeStatement();
        if (context.TABLE() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.TABLE;
            purge.Table = (Table)Visit(context.table());
        }
        else if (context.INDEX() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.INDEX;
            // INDEX 形式：table.identifier，取 table 作为宿主
            purge.Table = (Table)Visit(context.table());
        }
        else if (context.RECYCLEBIN() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.RECYCLEBIN;
        }
        else if (context.DBA_RECYCLEBIN() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.DBA_RECYCLEBIN;
        }
        else if (context.TABLESPACE() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.TABLESPACE;
            var ids = context.identifier();
            if (ids.Length > 0) purge.TableSpaceName = ids[0].GetText();
            if (context.USER() != null && ids.Length > 1) purge.UserName = ids[1].GetText();
        }
        return purge;
    }

    public override object VisitAlterViewStatement(JSqlParserGrammar.AlterViewStatementContext context)
    {
        var view = new AlterView
        {
            UseReplace = context.REPLACE() != null,
            View = (Table)Visit(context.table()),
            Select = (Select)Visit(context.selectStatement())
        };
        return view;
    }

    public override object VisitAlterSessionStatement(JSqlParserGrammar.AlterSessionStatementContext context)
    {
        var stmt = new AlterSession();
        var ids = context.identifier();
        if (context.SET() != null)
        {
            stmt.Operation = "SET";
            foreach (var id in ids) stmt.Parameters.Add(id.GetText());
        }
        else if (ids.Length > 0)
        {
            stmt.Operation = ids[0].GetText();
            for (var i = 1; i < ids.Length; i++) stmt.Parameters.Add(ids[i].GetText());
        }
        return stmt;
    }

    public override object VisitAlterSystemStatement(JSqlParserGrammar.AlterSystemStatementContext context)
    {
        var stmt = new AlterSystemStatement();
        var ids = context.identifier();
        if (context.SET() != null)
        {
            stmt.Operation = "SET";
            foreach (var id in ids) stmt.Parameters.Add(id.GetText());
        }
        else if (ids.Length > 0)
        {
            stmt.Operation = ids[0].GetText();
            for (var i = 1; i < ids.Length; i++) stmt.Parameters.Add(ids[i].GetText());
        }
        return stmt;
    }

    public override object VisitAlterSequenceStatement(JSqlParserGrammar.AlterSequenceStatementContext context)
    {
        var table = (Table)Visit(context.table());
        var sequence = new Sequence { Name = table.Name, SchemaName = table.SchemaName, Database = table.Database };
        var stmt = new AlterSequence { Sequence = sequence };
        foreach (var optCtx in context.alterSequenceOption())
        {
            sequence.Parameters ??= new List<SequenceParameter>();
            var param = ParseAlterSequenceOption(optCtx);
            if (param != null) sequence.Parameters.Add(param);
        }
        return stmt;
    }

    /// <summary>
    /// 将单个 alterSequenceOption 上下文解析为结构化 <see cref="SequenceParameter"/>。
    /// </summary>
    private static SequenceParameter? ParseAlterSequenceOption(JSqlParserGrammar.AlterSequenceOptionContext ctx)
    {
        if (ctx.RESTART() != null)
        {
            var p = new SequenceParameter(SequenceParameterType.RESTART_WITH);
            if (ctx.WITH() != null && ctx.LONG_VALUE() != null)
                p.Value = ParseLong(ctx.LONG_VALUE().GetText());
            return p;
        }
        if (ctx.INCREMENT() != null && ctx.LONG_VALUE() != null)
            return new SequenceParameter(SequenceParameterType.INCREMENT_BY).WithValue(ParseLong(ctx.LONG_VALUE().GetText()));
        if (ctx.NOMINVALUE() != null) return new SequenceParameter(SequenceParameterType.NOMINVALUE);
        if (ctx.MINVALUE() != null && ctx.LONG_VALUE() != null)
            return new SequenceParameter(SequenceParameterType.MINVALUE).WithValue(ParseLong(ctx.LONG_VALUE().GetText()));
        if (ctx.NOMAXVALUE() != null) return new SequenceParameter(SequenceParameterType.NOMAXVALUE);
        if (ctx.MAXVALUE() != null && ctx.LONG_VALUE() != null)
            return new SequenceParameter(SequenceParameterType.MAXVALUE).WithValue(ParseLong(ctx.LONG_VALUE().GetText()));
        if (ctx.NOCACHE() != null) return new SequenceParameter(SequenceParameterType.NOCACHE);
        if (ctx.CACHE() != null && ctx.LONG_VALUE() != null)
            return new SequenceParameter(SequenceParameterType.CACHE).WithValue(ParseLong(ctx.LONG_VALUE().GetText()));
        if (ctx.NOCYCLE() != null) return new SequenceParameter(SequenceParameterType.NOCYCLE);
        if (ctx.CYCLE() != null) return new SequenceParameter(SequenceParameterType.CYCLE);
        if (ctx.NOORDER() != null) return new SequenceParameter(SequenceParameterType.NOORDER);
        if (ctx.ORDER() != null) return new SequenceParameter(SequenceParameterType.ORDER);
        return null;
    }

    public override object VisitCreateSynonymStatement(JSqlParserGrammar.CreateSynonymStatementContext context)
    {
        var stmt = new CreateSynonym
        {
            OrReplace = context.OR() != null && context.REPLACE() != null,
            PublicSynonym = context.PUBLIC() != null,
            Name = context.identifier(0).GetText()
        };
        // FOR target：grammar FOR identifier (DOT identifier)?
        if (context.FOR() != null)
        {
            var forIds = context.identifier();
            // 第 0 个是 synonym 名，FOR 后的从 index 1 开始
            if (forIds.Length > 1)
            {
                var target = forIds[1].GetText();
                if (forIds.Length > 2) target += $".{forIds[2].GetText()}";
                stmt.ForList.Add(target);
            }
        }
        return stmt;
    }

    public override object VisitBlockStatement(JSqlParserGrammar.BlockStatementContext context)
    {
        var block = new Block();
        foreach (var stmtCtx in context.statement())
        {
            block.Statements.StatementList.Add((Azrng.JSqlParser.Statement.IStatement)Visit(stmtCtx));
        }
        return block;
    }

    public override object VisitDeclareStatement(JSqlParserGrammar.DeclareStatementContext context)
    {
        var stmt = new DeclareStatement();
        foreach (var itemCtx in context.declareItem())
        {
            // 变量名可能来自 identifier、SINGLE_AT_IDENTIFIER 或 S_AT_IDENTIFIER
            var varName = itemCtx.identifier()?.GetText()
                ?? itemCtx.SINGLE_AT_IDENTIFIER()?.GetText()
                ?? itemCtx.S_AT_IDENTIFIER()?.GetText() ?? "";
            var item = new TypeDefExpr
            {
                UserVariable = varName,
                Type = itemCtx.dataType().GetText()
            };
            if (itemCtx.expression() != null)
                item.DefaultExpression = (Expression.IExpression)Visit(itemCtx.expression());
            stmt.TypeDefExprList.Add(item);
        }
        return stmt;
    }

    public override object VisitIfElseStatement(JSqlParserGrammar.IfElseStatementContext context)
    {
        var stmt = new IfElseStatement
        {
            Condition = (Expression.IExpression)Visit(context.expression()),
            IfStatement = (Azrng.JSqlParser.Statement.IStatement)Visit(context.statement(0))
        };
        if (context.statement().Length > 1)
            stmt.ElseStatement = (Azrng.JSqlParser.Statement.IStatement)Visit(context.statement(1));
        return stmt;
    }

    public override object VisitCreateFunctionStatement(JSqlParserGrammar.CreateFunctionStatementContext context)
    {
        var orReplace = context.OR() != null && context.REPLACE() != null;
        // 收集 functionBodyTokens 的原始文本（对齐上游 captureFunctionBody 容器式行为）
        var parts = new List<string> { context.identifier().GetText() };
        parts.AddRange(context.functionBodyTokens().GetText().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (context.FUNCTION() != null)
        {
            return new CreateFunction(parts) { OrReplace = orReplace };
        }
        return new CreateProcedure(parts) { OrReplace = orReplace };
    }

    /// <summary>
    /// 将 alterOperation 文法节点转换为 AlterExpression。修复此前 alterOperation 完全未解析的缺陷。
    /// </summary>
    private AlterExpression BuildAlterExpression(JSqlParserGrammar.AlterOperationContext context)
    {
        var expr = new AlterExpression();
        var identifiers = context.identifier();

        // 分区操作优先识别（ADD/DROP/... PARTITION 与列操作共享 ADD/DROP token，须先分流）
        if (context.PARTITION() != null)
        {
            BuildPartitionOperation(expr, context);
            return expr;
        }

        // ADD COLUMN? columnDefinition | ADD tableConstraint
        if (context.ADD() != null)
        {
            expr.Operation = AlterOperation.Add;
            if (context.tableConstraint() != null)
            {
                // ADD 约束：约束类型/列/USING INDEX 写入结构化字段
                var constraint = (Constraint)Visit(context.tableConstraint());
                expr.ConstraintType = constraint.Type;
                if (constraint.Name != null) expr.ConstraintSymbol = constraint.Name;
                if (constraint.Type == "PRIMARY KEY" && constraint.Columns.Count > 0)
                    expr.PkColumns = constraint.Columns;
                else if (constraint.Type == "UNIQUE" && constraint.Columns.Count > 0)
                    expr.UkColumns = constraint.Columns;
                else if (!string.IsNullOrEmpty(constraint.Type))
                {
                    // KEY/INDEX 等 MySQL 索引约束：用 Constraint.ToString 作为可选说明符输出
                    expr.OptionalSpecifier = constraint.ToString();
                    // 清空 ConstraintType 避免 ToString 重复输出约束关键字
                    expr.ConstraintType = null;
                }
                // USING INDEX 子句
                expr.HasUsingIndex = constraint.HasUsingIndex;
                expr.UsingIndex = constraint.UsingIndex;
            }
            else if (context.columnDefinition() != null)
            {
                var colDef = (ColumnDefinition)Visit(context.columnDefinition());
                expr.ColDataTypeList = new List<AlterExpression.ColumnDataType>
                {
                    new() { ColumnName = colDef.ColumnName, DataType = colDef.ColDataType.ToString() }
                };
            }
            return expr;
        }

        // DROP 分支：DROP COLUMN / DROP PRIMARY KEY / DROP UNIQUE / DROP FOREIGN KEY / DROP CONSTRAINT
        // 注：排除 ALTER COLUMN ... DROP DEFAULT/DROP NOT NULL（ALTER 分支内含 DROP token，但操作类型是 ALTER）
        if (context.DROP() != null && context.ALTER() == null)
        {
            if (context.PRIMARY() != null)
            {
                expr.Operation = AlterOperation.DropPrimaryKey;
            }
            else if (context.UNIQUE() != null)
            {
                expr.Operation = AlterOperation.DropUnique;
                if (identifiers.Length > 0) expr.ConstraintSymbol = identifiers[0].GetText();
            }
            else if (context.FOREIGN() != null)
            {
                expr.Operation = AlterOperation.DropForeignKey;
                if (identifiers.Length > 0) expr.ConstraintSymbol = identifiers[0].GetText();
            }
            else if (context.CONSTRAINT() != null)
            {
                expr.Operation = AlterOperation.Drop;
                if (identifiers.Length > 0) expr.ConstraintSymbol = identifiers[0].GetText();
            }
            else
            {
                expr.Operation = AlterOperation.Drop;
                if (identifiers.Length > 0)
                    expr.ColumnName = identifiers[0].GetText();
            }
            return expr;
        }

        // MODIFY COLUMN? columnDefinition
        if (context.MODIFY() != null)
        {
            expr.Operation = AlterOperation.Modify;
            if (context.columnDefinition() != null)
            {
                var colDef = (ColumnDefinition)Visit(context.columnDefinition());
                expr.ColDataTypeList = new List<AlterExpression.ColumnDataType>
                {
                    new() { ColumnName = colDef.ColumnName, DataType = colDef.ColDataType.ToString() }
                };
            }
            return expr;
        }

        // CHANGE COLUMN? identifier columnDefinition
        if (context.CHANGE() != null)
        {
            expr.Operation = AlterOperation.Change;
            if (identifiers.Length > 0)
                expr.ColumnOldName = identifiers[0].GetText();
            if (context.columnDefinition() != null)
            {
                var colDef = (ColumnDefinition)Visit(context.columnDefinition());
                expr.ColDataTypeList = new List<AlterExpression.ColumnDataType>
                {
                    new() { ColumnName = colDef.ColumnName, DataType = colDef.ColDataType.ToString() }
                };
            }
            return expr;
        }

        // ALTER COLUMN? identifier (SET DEFAULT ... | DROP DEFAULT | SET NOT NULL | DROP NOT NULL | TYPE ...)
        if (context.ALTER() != null)
        {
            expr.Operation = AlterOperation.Alter;
            expr.UseColumnKeyword = context.COLUMN() != null;
            if (identifiers.Length > 0)
                expr.ColumnName = identifiers[0].GetText();
            // 接线 ALTER COLUMN 子句（此前静默丢弃，对齐上游）
            if (context.DEFAULT() != null && context.SET() != null)
            {
                expr.ColumnAlterAction = AlterColumnAction.SetDefault;
                if (context.expression() != null)
                    expr.AlterColumnDefaultExpression = GetOriginalText(context.expression());
            }
            else if (context.DEFAULT() != null && context.DROP() != null)
            {
                expr.ColumnAlterAction = AlterColumnAction.DropDefault;
            }
            else if (context.NOT() != null && context.SET() != null)
            {
                expr.ColumnAlterAction = AlterColumnAction.SetNotNull;
            }
            else if (context.NOT() != null && context.DROP() != null)
            {
                expr.ColumnAlterAction = AlterColumnAction.DropNotNull;
            }
            else if (context.TYPE() != null && context.DATA() != null)
            {
                expr.ColumnAlterAction = AlterColumnAction.SetDataType;
                if (context.dataType() != null)
                    expr.AlterColumnType = GetOriginalText(context.dataType());
            }
            else if (context.TYPE() != null)
            {
                expr.ColumnAlterAction = AlterColumnAction.Type;
                if (context.dataType() != null)
                    expr.AlterColumnType = GetOriginalText(context.dataType());
            }
            else if (context.VISIBLE() != null)
            {
                expr.ColumnAlterAction = AlterColumnAction.SetVisible;
            }
            else if (context.INVISIBLE() != null)
            {
                expr.ColumnAlterAction = AlterColumnAction.SetInvisible;
            }
            return expr;
        }

        // RENAME 分支：RENAME COLUMN? old TO new / RENAME TO table / RENAME INDEX/KEY/CONSTRAINT old TO new
        if (context.RENAME() != null)
        {
            if (context.INDEX() != null)
            {
                expr.Operation = AlterOperation.RenameIndex;
                if (identifiers.Length >= 2)
                {
                    expr.ColumnOldName = identifiers[0].GetText();
                    expr.ColumnName = identifiers[1].GetText();
                }
            }
            else if (context.KEY() != null)
            {
                expr.Operation = AlterOperation.RenameKey;
                if (identifiers.Length >= 2)
                {
                    expr.ColumnOldName = identifiers[0].GetText();
                    expr.ColumnName = identifiers[1].GetText();
                }
            }
            else if (context.CONSTRAINT() != null)
            {
                expr.Operation = AlterOperation.RenameConstraint;
                if (identifiers.Length >= 2)
                {
                    expr.ColumnOldName = identifiers[0].GetText();
                    expr.ColumnName = identifiers[1].GetText();
                }
            }
            else if (context.TO() != null)
            {
                if (identifiers.Length >= 2)
                {
                    // RENAME COLUMN? old TO new
                    expr.Operation = AlterOperation.Rename;
                    expr.ColumnOldName = identifiers[0].GetText();
                    expr.ColumnName = identifiers[1].GetText();
                }
                else if (identifiers.Length == 1)
                {
                    // RENAME TO new_table
                    expr.Operation = AlterOperation.RenameTable;
                    expr.NewTableName = identifiers[0].GetText();
                }
            }
            return expr;
        }

        // ROW LEVEL SECURITY 分支
        if (context.ENABLE() != null)
        {
            expr.Operation = AlterOperation.EnableRowLevelSecurity;
            return expr;
        }
        if (context.DISABLE() != null)
        {
            expr.Operation = AlterOperation.DisableRowLevelSecurity;
            return expr;
        }
        if (context.FORCE() != null)
        {
            expr.Operation = AlterOperation.ForceRowLevelSecurity;
            return expr;
        }
        if (context.NO() != null)
        {
            expr.Operation = AlterOperation.NoForceRowLevelSecurity;
            return expr;
        }

        // ENGINE [=] name
        if (context.ENGINE() != null)
        {
            expr.Operation = AlterOperation.Engine;
            expr.UseEqualsForEngine = context.EQUALS() != null;
            if (identifiers.Length > 0) expr.OptionalSpecifier = identifiers[0].GetText();
            return expr;
        }

        // COMMENT [=] 'xxx'
        if (context.COMMENT() != null)
        {
            expr.Operation = context.EQUALS() != null ? AlterOperation.CommentWithEqualSign : AlterOperation.Comment;
            expr.UseEqualsForComment = context.EQUALS() != null;
            if (context.S_CHAR_LITERAL() != null) expr.OptionalSpecifier = context.S_CHAR_LITERAL().GetText();
            return expr;
        }

        // CONVERT TO CHARACTER SET x [COLLATE [=] y] / DEFAULT CHARACTER SET x / CHARACTER SET x
        if (context.CONVERT() != null || context.CHARACTER() != null)
        {
            expr.Operation = context.CONVERT() != null ? AlterOperation.Convert : AlterOperation.Collate;
            expr.DefaultCollateSpecified = context.DEFAULT() != null; // 区分 DEFAULT CHARACTER SET
            // CHARACTER SET 后的 identifier 是字符集名（grammar 有 3 个 identifier 分支，取 CHARACTER SET 后的那个）
            var charSetIds = context.identifier();
            if (charSetIds.Length > 0) expr.CharacterSet = charSetIds[0].GetText();
            if (context.COLLATE() != null && charSetIds.Length > 1)
            {
                expr.Collation = charSetIds[1].GetText();
                expr.UseEqualsForComment = context.EQUALS() != null; // 复用标记记录 COLLATE 是否带等号
            }
            return expr;
        }

        if (context.REMOVE() != null)
        {
            expr.Operation = AlterOperation.RemovePartitioning;
            return expr;
        }

        expr.Operation = AlterOperation.Unspecific;
        return expr;
    }

    /// <summary>
    /// 识别分区操作并填充 <paramref name="expr"/> 的结构与 Operation 字段。
    /// </summary>
    private void BuildPartitionOperation(AlterExpression expr, JSqlParserGrammar.AlterOperationContext context)
    {
        if (context.ADD() != null)
        {
            expr.Operation = AlterOperation.AddPartition;
            var partDefs = context.partitionDef();
            if (partDefs != null && partDefs.Length > 0)
            {
                expr.PartitionDefinitions = new List<PartitionDefinition>();
                foreach (var pd in partDefs)
                    expr.PartitionDefinitions.Add(BuildPartitionDefinition(pd));
            }
        }
        else if (context.DROP() != null)
        {
            expr.Operation = AlterOperation.DropPartition;
            expr.PartitionNames = CollectIdentifiers(context.identifierList());
        }
        else if (context.TRUNCATE() != null)
        {
            expr.Operation = AlterOperation.TruncatePartition;
            expr.PartitionNames = CollectIdentifiers(context.identifierList());
        }
        else if (context.COALESCE() != null)
        {
            expr.Operation = AlterOperation.CoalescePartition;
            if (context.LONG_VALUE() != null)
                expr.CoalescePartitionNumber = ParseInt(context.LONG_VALUE().GetText());
        }
        else if (context.REORGANIZE() != null)
        {
            expr.Operation = AlterOperation.ReorganizePartition;
            expr.PartitionNames = CollectIdentifiers(context.identifierList());
            var partDefs = context.partitionDef();
            if (partDefs != null && partDefs.Length > 0)
            {
                expr.PartitionDefinitions = new List<PartitionDefinition>();
                foreach (var pd in partDefs)
                    expr.PartitionDefinitions.Add(BuildPartitionDefinition(pd));
            }
        }
        else if (context.EXCHANGE() != null)
        {
            expr.Operation = AlterOperation.ExchangePartition;
            var exchangeIdent = context.identifier();
            if (exchangeIdent != null && exchangeIdent.Length > 0)
                expr.PartitionNames = new List<string> { exchangeIdent[0].GetText() };
            if (context.table() != null)
            {
                var exTable = (Table)Visit(context.table());
                expr.ExchangePartitionTable = exTable.GetFullyQualifiedName();
            }
        }
        else
        {
            expr.Operation = AlterOperation.PartitionBy;
        }
    }

    /// <summary>
    /// 从 partitionDef 文法节点构建 <see cref="PartitionDefinition"/>。
    /// partitionDef: name VALUES? (LESS THAN (expr) | IN (exprList))?
    /// </summary>
    private PartitionDefinition BuildPartitionDefinition(JSqlParserGrammar.PartitionDefContext ctx)
    {
        var def = new PartitionDefinition();
        if (ctx.partitionName != null) def.Name = ctx.partitionName.GetText();
        if (ctx.LESS() != null && ctx.expression() != null)
        {
            def.ValuesLessThan = new ExpressionList
            {
                Expressions = new() { (Expression.IExpression)Visit(ctx.expression()) }
            };
        }
        else if (ctx.IN() != null && ctx.expressionList() != null)
        {
            def.ValuesIn = (ExpressionList)Visit(ctx.expressionList());
        }
        return def;
    }

    /// <summary>
    /// 从 identifierList 文法节点收集标识符文本列表。
    /// </summary>
    private static List<string> CollectIdentifiers(JSqlParserGrammar.IdentifierListContext? ctx)
    {
        var result = new List<string>();
        if (ctx == null) return result;
        foreach (var id in ctx.identifier()) result.Add(id.GetText());
        return result;
    }

    public override object VisitDropStatement(JSqlParserGrammar.DropStatementContext context)
    {
        var drop = new Drop();
        drop.Name = (Table)Visit(context.table(0));
        drop.Type = context.TABLE() != null ? "TABLE" :
                    context.VIEW() != null ? "VIEW" : "INDEX";
        drop.IfExists = context.IF() != null;
        return drop;
    }

    public override object VisitTruncateStatement(JSqlParserGrammar.TruncateStatementContext context)
    {
        var truncate = new Truncate();
        truncate.Table = (Table)Visit(context.table());
        return truncate;
    }

    public override object VisitCreatePolicy(JSqlParserGrammar.CreatePolicyContext context)
    {
        var policy = new CreatePolicy
        {
            PolicyName = context.identifier(0).GetText(),
            Table = (Table)Visit(context.table())
        };

        // FOR { ALL | SELECT | INSERT | UPDATE | DELETE }
        if (context.FOR() != null)
        {
            policy.Command = context.ALL() != null ? "ALL"
                : context.SELECT() != null ? "SELECT"
                : context.INSERT() != null ? "INSERT"
                : context.UPDATE() != null ? "UPDATE"
                : context.DELETE() != null ? "DELETE" : null;
        }

        // TO role1, role2, ...（identifier 列表，跳过第 0 个即 policyName）
        if (context.TO() != null)
        {
            var allIdentifiers = context.identifier();
            // 第 0 个是 policyName，从第 1 个开始是角色
            for (int i = 1; i < allIdentifiers.Length; i++)
            {
                policy.Roles.Add(allIdentifiers[i].GetText());
            }
        }

        // USING ( expression )
        // 注意：context.expression() 返回所有层级的 expression，USING 的表达式是第 1 个
        var expressions = context.expression();
        if (context.USING() != null && expressions.Length > 0)
        {
            policy.UsingExpression = (Expression.IExpression)Visit(expressions[0]);
        }

        // WITH CHECK ( expression )
        if (context.CHECK() != null)
        {
            var checkExprIdx = context.USING() != null ? 1 : 0;
            if (expressions.Length > checkExprIdx)
            {
                policy.WithCheckExpression = (Expression.IExpression)Visit(expressions[checkExprIdx]);
            }
        }

        return policy;
    }

    public override object VisitCreateSequence(JSqlParserGrammar.CreateSequenceContext context)
    {
        var table = (Table)Visit(context.table());
        var sequence = new Schema.Sequence
        {
            Database = table.Database,
            SchemaName = table.SchemaName,
            Name = table.Name
        };

        if (context.sequenceParameter().Length > 0)
        {
            sequence.Parameters = new List<SequenceParameter>();
            foreach (var paramCtx in context.sequenceParameter())
            {
                sequence.Parameters.Add(BuildSequenceParameter(paramCtx));
            }
        }

        return new CreateSequence { Sequence = sequence };
    }

    private static SequenceParameter BuildSequenceParameter(JSqlParserGrammar.SequenceParameterContext ctx)
    {
        // 有值参数
        if (ctx.INCREMENT() != null)
        {
            var value = ParseLong(ctx.LONG_VALUE().GetText());
            return new SequenceParameter(
                ctx.BY() != null ? SequenceParameterType.INCREMENT_BY : SequenceParameterType.INCREMENT)
                .WithValue(value);
        }
        if (ctx.START() != null)
        {
            var value = ctx.LONG_VALUE() != null ? ParseLong(ctx.LONG_VALUE().GetText()) : (long?)null;
            return new SequenceParameter(
                ctx.WITH() != null ? SequenceParameterType.START_WITH : SequenceParameterType.START)
                .WithValue(value ?? 0);
        }
        if (ctx.RESTART() != null)
        {
            var value = ctx.LONG_VALUE() != null ? ParseLong(ctx.LONG_VALUE().GetText()) : (long?)null;
            var p = new SequenceParameter(SequenceParameterType.RESTART_WITH);
            if (value != null) p.WithValue(value.Value);
            return p;
        }
        if (ctx.MAXVALUE() != null) return new SequenceParameter(SequenceParameterType.MAXVALUE).WithValue(ParseLong(ctx.LONG_VALUE().GetText()));
        if (ctx.MINVALUE() != null) return new SequenceParameter(SequenceParameterType.MINVALUE).WithValue(ParseLong(ctx.LONG_VALUE().GetText()));
        if (ctx.CACHE() != null) return new SequenceParameter(SequenceParameterType.CACHE).WithValue(ParseLong(ctx.LONG_VALUE().GetText()));

        // 无值参数
        if (ctx.NOMAXVALUE() != null) return new SequenceParameter(SequenceParameterType.NOMAXVALUE);
        if (ctx.NOMINVALUE() != null) return new SequenceParameter(SequenceParameterType.NOMINVALUE);
        if (ctx.CYCLE() != null) return new SequenceParameter(SequenceParameterType.CYCLE);
        if (ctx.NOCYCLE() != null) return new SequenceParameter(SequenceParameterType.NOCYCLE);
        if (ctx.NOCACHE() != null) return new SequenceParameter(SequenceParameterType.NOCACHE);
        if (ctx.ORDER() != null) return new SequenceParameter(SequenceParameterType.ORDER);
        if (ctx.NOORDER() != null) return new SequenceParameter(SequenceParameterType.NOORDER);
        if (ctx.KEEP() != null) return new SequenceParameter(SequenceParameterType.KEEP);
        if (ctx.NOKEEP() != null) return new SequenceParameter(SequenceParameterType.NOKEEP);

        return new SequenceParameter();
    }

    public override object VisitCreateSchema(JSqlParserGrammar.CreateSchemaContext context)
    {
        var schema = new Statement.Create.Schema.CreateSchema();
        if (context.IF() != null) schema.IfNotExists = true;

        var qCtx = context.schemaQualifiedName();
        var nameParts = qCtx.identifier();
        if (nameParts.Length == 1)
        {
            schema.SchemaName = nameParts[0].GetText();
        }
        else if (nameParts.Length == 2)
        {
            // catalog.schema 形式
            schema.CatalogName = nameParts[0].GetText();
            schema.SchemaName = nameParts[1].GetText();
        }

        if (context.AUTHORIZATION() != null)
        {
            // createSchema 文法中 AUTHORIZATION 后的 identifier 是唯一直接子节点 identifier
            var authId = context.identifier();
            if (authId != null) schema.Authorization = authId.GetText();
        }
        return schema;
    }

    public override object VisitRefreshStatement(JSqlParserGrammar.RefreshStatementContext context)
    {
        var refresh = new Statement.Refresh.RefreshMaterializedViewStatement();
        if (context.table() is { } viewCtx) refresh.View = (Table)Visit(viewCtx);
        if (context.CONCURRENTLY() != null) refresh.Concurrently = true;
        // WITH [NO] DATA
        if (context.WITH() != null && context.DATA() != null)
        {
            refresh.RefreshMode = context.NO() != null
                ? Statement.Refresh.RefreshMode.WithNoData
                : Statement.Refresh.RefreshMode.WithData;
        }
        return refresh;
    }

    public override object VisitCreateView(JSqlParserGrammar.CreateViewContext context)
    {
        var createView = new CreateView();
        createView.View = (Table)Visit(context.table());
        createView.OrReplace = context.REPLACE() != null;
        createView.IfNotExists = context.EXISTS() != null;
        // FORCE / NO FORCE（Oracle）
        if (context.FORCE() != null && context.NO() == null) createView.Force = true;
        else if (context.FORCE() != null && context.NO() != null) createView.Force = false;
        // TEMPORARY/TEMP（此前 grammar 已解析但 visitor 丢弃）
        if (context.TEMPORARY() != null) createView.Temporary = "TEMPORARY";
        else if (context.TEMP() != null) createView.Temporary = "TEMP";
        // RECURSIVE
        if (context.RECURSIVE() != null) createView.Recursive = true;
        // SECURE（Snowflake/SAP HANA）
        if (context.SECURE() != null) createView.Secure = true;
        // WITH [CASCADED|LOCAL] CHECK OPTION（此前丢弃）
        if (context.CHECK() != null)
        {
            createView.WithCheckOption = context.CASCADED() != null ? "CASCADED"
                : context.LOCAL() != null ? "LOCAL" : "";
        }
        // WITH READ ONLY（Oracle）
        if (context.READ() != null && context.ONLY() != null) createView.WithReadOnly = true;
        createView.Select = (Select)Visit(context.selectStatement());
        return createView;
    }

    public override object VisitCreateIndex(JSqlParserGrammar.CreateIndexContext context)
    {
        var createIndex = new CreateIndex();
        createIndex.Index = new Schema.Index { Name = context.identifier(0).GetText() };
        createIndex.Table = (Table)Visit(context.table());
        createIndex.Unique = context.UNIQUE() != null;
        // PostgreSQL 索引方法 USING btree|gist|gin|...
        if (context.USING() != null)
            createIndex.UsingMethod = context.identifier(1).GetText();
        // 索引列（含 ASC/DESC/表达式/opclass），取原始文本保 round-trip
        foreach (var item in context.orderByItem())
            createIndex.ColumnNames.Add(GetOriginalText(item));
        // 部分索引 WHERE
        if (context.whereClause() != null)
            createIndex.Where = (Expression.IExpression)Visit(context.whereClause().expression());
        return createIndex;
    }
}
