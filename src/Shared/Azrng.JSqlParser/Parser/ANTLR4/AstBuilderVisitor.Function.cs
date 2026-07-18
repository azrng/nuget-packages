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
/// AstBuilderVisitor 的 Function 分部：JSON / XML / 转码 / 字符串 / GROUP_CONCAT 等函数族。
/// </summary>
public partial class AstBuilderVisitor
{
    public override object VisitJsonTable(JSqlParserGrammar.JsonTableContext context)
    {
        var jsonTable = new JsonTable
        {
            JsonExpression = (Expression.IExpression)Visit(context.expression())
        };

        // 输入 FORMAT JSON（Oracle）
        if (context.FORMAT() != null) jsonTable.InputFormatJson = true;

        // path：第二个参数的 S_CHAR_LITERAL（在 expression 之后、PASSING 之前）
        if (context.COMMA().Length > 0 && context.S_CHAR_LITERAL() != null)
        {
            jsonTable.PathExpression = context.S_CHAR_LITERAL().GetText();
        }

        // PASSING 子句
        foreach (var p in context.jsonTablePassingItem())
        {
            jsonTable.PassingClauses.Add((JsonTablePassingClause)Visit(p));
        }

        // TYPE (STRICT|LAX)
        if (context.TYPE() != null)
        {
            jsonTable.ParsingType = context.STRICT() != null ? "STRICT" : "LAX";
        }

        // ON EMPTY / ON ERROR（jsonTableBehavior + ON EMPTY_KW/ON ERROR）
        var behaviors = context.jsonTableBehavior();
        bool hasEmpty = context.EMPTY_KW() != null;
        if (behaviors.Length >= 1 && hasEmpty)
        {
            jsonTable.OnEmptyBehavior = ParseJsonTableBehavior(behaviors[0]);
        }
        if (behaviors.Length >= 2 || (behaviors.Length >= 1 && !hasEmpty))
        {
            // 无 ON EMPTY 时，唯一 behavior 是 ON ERROR；有 ON EMPTY 时第二个是 ON ERROR
            jsonTable.OnErrorBehavior = ParseJsonTableBehavior(hasEmpty ? behaviors[1] : behaviors[0]);
        }

        foreach (var colCtx in context.jsonTableColumn())
        {
            jsonTable.Columns.Add((JsonTableColumn)Visit(colCtx));
        }

        // PLAN [DEFAULT] (plan_expr)
        if (context.jsonTablePlanClause() != null)
        {
            jsonTable.Plan = context.jsonTablePlanClause().GetText();
        }

        return jsonTable;
    }

    /// <summary>将 jsonTableBehavior 上下文解析为 JsonOnResponseBehavior。</summary>
    private JsonFunction.JsonOnResponseBehavior ParseJsonTableBehavior(JSqlParserGrammar.JsonTableBehaviorContext ctx)
    {
        if (ctx.ERROR() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.ERROR);
        if (ctx.NULL() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.NULL);
        if (ctx.TRUE() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.TRUE);
        if (ctx.FALSE() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.FALSE);
        if (ctx.EMPTY_KW() != null)
        {
            return ctx.ARRAY() != null
                ? new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY_ARRAY)
                : ctx.OBJECT() != null
                    ? new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY_OBJECT)
                    : new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY);
        }
        if (ctx.DEFAULT() != null)
        {
            return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.DEFAULT,
                (Expression.IExpression)Visit(ctx.expression()));
        }
        return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.NULL);
    }

    public override object VisitJsonTablePassingItem(JSqlParserGrammar.JsonTablePassingItemContext context)
    {
        return new JsonTablePassingClause
        {
            ValueExpression = (Expression.IExpression)Visit(context.expression()),
            ParameterName = context.identifier().GetText()
        };
    }

    public override object VisitJsonTableColumn(JSqlParserGrammar.JsonTableColumnContext context)
    {
        // NESTED PATH 分支
        if (context.NESTED() != null)
        {
            var nested = new JsonTableColumn
            {
                Path = context.S_CHAR_LITERAL().GetText(),
                NestedColumns = new List<JsonTableColumn>()
            };
            foreach (var inner in context.jsonTableColumn())
            {
                nested.NestedColumns.Add((JsonTableColumn)Visit(inner));
            }
            return nested;
        }

        // 列名是首个 identifier（grammar 含列名 + 可选 ENCODING name 两个 identifier）
        var colIdents = context.identifier();
        var column = new JsonTableColumn { Name = colIdents[0].GetText() };

        if (context.FOR() != null)
        {
            column.ForOrdinality = true;
        }
        else
        {
            // dataType 是组合 token，取原始文本
            column.DataType = context.dataType().GetText();
            if (context.EXISTS() != null) column.Exists = true;
            if (context.PATH() != null)
            {
                column.Path = context.S_CHAR_LITERAL().GetText();
            }
            // FORMAT JSON [ENCODING name]
            if (context.FORMAT() != null)
            {
                column.FormatJson = true;
                if (context.ENCODING() != null) column.Encoding = colIdents.Length > 1 ? colIdents[1].GetText() : "";
            }
            // WRAPPER
            if (context.jsonWrapperClause() != null)
            {
                var w = context.jsonWrapperClause();
                column.Wrapper = w.WITHOUT() != null
                    ? JsonFunction.WrapperType.WITHOUT
                    : JsonFunction.WrapperType.WITH;
                if (w.CONDITIONAL() != null) column.WrapperMode = JsonFunction.WrapperMode.CONDITIONAL;
                if (w.UNCONDITIONAL() != null) column.WrapperMode = JsonFunction.WrapperMode.UNCONDITIONAL;
                if (w.ARRAY() != null) column.WrapperArray = true;
            }
            // QUOTES
            if (context.jsonQuotesClause() != null)
            {
                var q = context.jsonQuotesClause();
                column.Quotes = q.KEEP() != null ? JsonFunction.QuotesType.KEEP : JsonFunction.QuotesType.OMIT;
                if (q.SCALAR() != null) column.QuotesOnScalarString = true;
            }
            // SCALARS
            if (context.ALLOW() != null) column.Scalars = JsonFunction.ScalarsType.ALLOW;
            else if (context.DISALLOW() != null) column.Scalars = JsonFunction.ScalarsType.DISALLOW;
            // 列级 ON EMPTY / ON ERROR
            var colBehaviors = context.jsonTableBehavior();
            bool colHasEmpty = context.EMPTY_KW() != null;
            if (colBehaviors.Length >= 1 && colHasEmpty)
            {
                column.OnEmptyBehavior = ParseJsonTableBehavior(colBehaviors[0]);
            }
            if (colBehaviors.Length >= 2 || (colBehaviors.Length >= 1 && !colHasEmpty))
            {
                column.OnErrorBehavior = ParseJsonTableBehavior(colHasEmpty ? colBehaviors[1] : colBehaviors[0]);
            }
        }

        return column;
    }

    public override object VisitXmlTable(JSqlParserGrammar.XmlTableContext context)
    {
        var xmlTable = new XmlTable
        {
            // 行 XPath 查询串（带引号原样保留，保 round-trip）
            RowPath = context.S_CHAR_LITERAL().GetText()
        };
        // 可选 XMLNAMESPACES(...) 前缀，原样保留
        if (context.xmlNamespacesClause() is { } ns)
        {
            xmlTable.XmlNamespaces = GetOriginalText(ns);
        }
        foreach (var expr in context.expression())
        {
            xmlTable.Passing.Add((Expression.IExpression)Visit(expr));
        }
        foreach (var colCtx in context.xmlTableColumn())
        {
            xmlTable.Columns.Add((XmlTableColumn)Visit(colCtx));
        }
        return xmlTable;
    }

    public override object VisitXmlTableColumn(JSqlParserGrammar.XmlTableColumnContext context)
    {
        var column = new XmlTableColumn { Name = context.identifier().GetText() };
        if (context.FOR() != null)
        {
            column.ForOrdinality = true;
            return column;
        }
        column.DataType = BuildColDataType(context.colDataType()).ToString();
        if (context.PATH() != null) column.Path = context.S_CHAR_LITERAL().GetText();
        if (context.DEFAULT() != null)
            column.DefaultExpression = (Expression.IExpression)Visit(context.expression());
        return column;
    }

    public override object VisitTrimFunction(JSqlParserGrammar.TrimFunctionContext context)
    {
        var trim = new TrimFunction();

        // 规范：LEADING/TRAILING/BOTH
        if (context.LEADING() != null) trim.TrimSpecification = TrimSpecification.Leading;
        else if (context.TRAILING() != null) trim.TrimSpecification = TrimSpecification.Trailing;
        else if (context.BOTH() != null) trim.TrimSpecification = TrimSpecification.Both;

        // 解析所有 expression，按位置赋值
        var exprs = context.expression();
        if (context.FROM() != null || context.COMMA() != null)
        {
            // [chars] [FROM|,] str 形式（2 个 expression）
            if (exprs.Length >= 1) trim.Expression = (Expression.IExpression)Visit(exprs[0]);
            if (exprs.Length >= 2) trim.FromExpression = (Expression.IExpression)Visit(exprs[1]);
            trim.UsingFromKeyword = context.FROM() != null;
        }
        else if (exprs.Length >= 1)
        {
            // TRIM(str) 简单形式（1 个 expression）
            trim.FromExpression = (Expression.IExpression)Visit(exprs[0]);
        }

        return trim;
    }

    // SQL Server 表提示：WITH (INDEX(name) | NOLOCK | ...)

    public override object VisitFunctionExpr(JSqlParserGrammar.FunctionExprContext context)
    {
        // MySQL GROUP_CONCAT 特殊语法分支（独立规则，便于无歧义地访问内部子句）
        // 对应上游 commit ff28f826。
        if (context.groupConcatFunction() != null)
        {
            return Visit(context.groupConcatFunction());
        }

        // CONVERT / TRY_CONVERT / SAFE_CONVERT 双风格转码函数
        if (context.transcodingFunction() != null)
        {
            return Visit(context.transcodingFunction());
        }

        // JSON 标量函数 JSON_OBJECT / JSON_ARRAY / JSON_VALUE / JSON_EXISTS
        if (context.jsonObjectFunction() != null)
        {
            return Visit(context.jsonObjectFunction());
        }
        if (context.jsonArrayFunction() != null)
        {
            return Visit(context.jsonArrayFunction());
        }
        if (context.jsonValueFunction() != null)
        {
            return Visit(context.jsonValueFunction());
        }
        if (context.jsonExistsFunction() != null)
        {
            return Visit(context.jsonExistsFunction());
        }
        if (context.jsonQueryFunction() != null)
        {
            return Visit(context.jsonQueryFunction());
        }
        if (context.jsonObjectAggFunction() != null)
        {
            return Visit(context.jsonObjectAggFunction());
        }
        if (context.jsonArrayAggFunction() != null)
        {
            return Visit(context.jsonArrayAggFunction());
        }

        // SQL 标准命名参数字符串函数 SUBSTRING(x FROM 1 FOR 3) / POSITION(a IN b) / OVERLAY(x PLACING y FROM 1)
        if (context.specialStringFunction() != null)
        {
            return Visit(context.specialStringFunction());
        }

        // 序列取值表达式：NEXTVAL FOR seq 或 NEXT VALUE FOR seq
        // （NEXTVAL(seq) PostgreSQL 风格继续按 Function 处理）
        if ((context.NEXTVAL() != null || context.NEXT() != null) && context.FOR() != null)
        {
            var col = (Column)Visit(context.columnRef());
            // 序列名可能是多段限定（schema.seq），按 . 拆分
            var fullName = col.GetFullyQualifiedName();
            var nameList = fullName.Split('.').ToList();
            return new NextValExpression(nameList, usingNextValueFor: context.NEXT() != null);
        }

        var funcName = context.identifier()?.GetText() ?? context.NEXTVAL()?.GetText() ?? "";

        ExpressionList? parameters = null;
        if (context.expressionList() != null)
        {
            parameters = new ExpressionList();
            parameters.Expressions = new List<Expression.IExpression>();
            foreach (var expr in context.expressionList().expression())
            {
                parameters.Expressions.Add((Expression.IExpression)Visit(expr));
            }
        }

        // 构造 AnalyticExpression 的条件：有 OVER，或有 WITHIN GROUP，或有 FILTER（对齐上游 AnalyticType 四态）
        bool hasOver = context.overClause() != null;
        bool hasWithinGroup = context.withinGroupClause() != null;
        bool hasFilter = context.filterClause() != null;
        if (hasOver || hasWithinGroup || hasFilter)
        {
            var analytic = new AnalyticExpression();
            analytic.Name = funcName;
            ApplyFunctionClauses(context, analytic);

            // 按 OVER/WITHIN GROUP/FILTER 组合设置 AnalyticType，对齐上游
            if (hasWithinGroup && hasOver) analytic.Type = AnalyticType.WithinGroupOver;
            else if (hasWithinGroup) analytic.Type = AnalyticType.WithinGroup;
            else if (hasFilter && !hasOver) analytic.Type = AnalyticType.FilterOnly;
            else analytic.Type = AnalyticType.Over;

            if (context.MULTIPLY() != null ||
                (parameters != null && parameters.Expressions.Count == 1 && parameters.Expressions[0] is AllColumns))
            {
                analytic.AllColumns = true;
            }
            else if (context.DISTINCT() != null && parameters != null)
            {
                analytic.Distinct = true;
                analytic.Expression = parameters.Expressions.Count > 0 ? parameters.Expressions[0] : null;
            }
            else if (parameters != null && parameters.Expressions.Count > 0)
            {
                analytic.Expression = parameters.Expressions[0];
                if (parameters.Expressions.Count > 1) analytic.Offset = parameters.Expressions[1];
                if (parameters.Expressions.Count > 2) analytic.DefaultValue = parameters.Expressions[2];
            }

            if (hasOver)
            {
                var overCtx = context.overClause();
                if (overCtx.identifier() != null)
                {
                    analytic.WindowName = overCtx.identifier().GetText();
                }
                else if (overCtx.windowSpecification() != null)
                {
                    var winSpec = overCtx.windowSpecification();
                    if (winSpec.PARTITION() != null)
                    {
                        analytic.PartitionExpressionList = new List<Expression.IExpression>();
                        foreach (var partExpr in winSpec.expression())
                        {
                            analytic.PartitionExpressionList.Add((Expression.IExpression)Visit(partExpr));
                        }
                    }
                    if (winSpec.orderByClause() != null)
                    {
                        analytic.OrderByElements = (List<OrderByElement>)Visit(winSpec.orderByClause());
                    }
                    if (winSpec.windowFrame() != null)
                    {
                        analytic.WindowFrame = (WindowFrame)Visit(winSpec.windowFrame());
                    }
                }
            }

            return analytic;
        }

        var func = new Function();
        func.Name = funcName;
        func.Parameters = parameters;
        func.AllColumns = context.MULTIPLY() != null ||
            (parameters != null && parameters.Expressions.Count == 1 && parameters.Expressions[0] is AllColumns);

        // 通用函数关键字参数（在 ) 之后）
        var keywordArgs = context.functionKeywordArgument();
        if (keywordArgs != null && keywordArgs.Length > 0)
        {
            func.KeywordArguments = new List<KeywordArgument>();
            foreach (var kaCtx in keywordArgs)
            {
                func.KeywordArguments.Add((KeywordArgument)Visit(kaCtx));
            }
        }

        // Oracle KEEP (DENSE_RANK FIRST|LAST ORDER BY ...)
        if (context.keepExpression() != null)
        {
            func.Keep = (KeepExpression)Visit(context.keepExpression());
        }

        ApplyFunctionClauses(context, func);
        return func;
    }

    // SQL 标准命名参数字符串函数：SUBSTRING(x FROM 1 FOR 3) / POSITION(a IN b) / OVERLAY(x PLACING y FROM 1)

    public override object VisitSpecialStringFunction(JSqlParserGrammar.SpecialStringFunctionContext context)
    {
        var func = new Function { Name = context.identifier().GetText() };
        var tail = context.namedFunctionParamTail();
        var named = new NamedExpressionList();

        // 首个表达式前缀为空；第二个表达式前缀为 FROM/IN/PLACING（specialStringFunction 中的命名关键字）
        var firstExpr = (Expression.IExpression)Visit(context.expression());
        var secondExpr = (Expression.IExpression)Visit(tail.expression(0));
        var firstKw = context.FROM() != null ? "FROM" : context.IN() != null ? "IN" : "PLACING";

        named.Expressions.Add(firstExpr);
        named.Names.Add("");
        named.Expressions.Add(secondExpr);
        named.Names.Add(firstKw);

        // 可选第三段（FROM/FOR）与第四段（FOR）：OVERLAY(x PLACING y FROM z FOR w)
        if (tail.expression().Length > 1)
        {
            named.Expressions.Add((Expression.IExpression)Visit(tail.expression(1)));
            named.Names.Add(tail.FROM() != null ? "FROM" : "FOR");
        }
        if (tail.expression().Length > 2)
        {
            named.Expressions.Add((Expression.IExpression)Visit(tail.expression(2)));
            named.Names.Add("FOR");
        }

        func.NamedParameters = named;
        return func;
    }

    // Oracle KEEP (DENSE_RANK FIRST|LAST ORDER BY ...)

    public override object VisitKeepExpression(JSqlParserGrammar.KeepExpressionContext context)
    {
        var keep = new KeepExpression
        {
            Name = context.identifier().GetText(),
            First = context.FIRST() != null,
            OrderByElements = (List<OrderByElement>)Visit(context.orderByClause())
        };
        return keep;
    }

    // 通用函数关键字参数：nonReservedKeyword expression

    public override object VisitFunctionKeywordArgument(JSqlParserGrammar.FunctionKeywordArgumentContext context)
    {
        var keyword = context.nonReservedKeyword().GetText();
        var arg = new KeywordArgument { Keyword = keyword };
        if (context.expression() != null)
        {
            arg.Expression = (Expression.IExpression)Visit(context.expression());
        }
        return arg;
    }

    private void ApplyFunctionClauses(JSqlParserGrammar.FunctionExprContext context, Function function)
    {
        if (context.withinGroupClause() != null)
        {
            function.WithinGroupOrderByElements =
                (List<OrderByElement>)Visit(context.withinGroupClause().orderByClause());
        }

        if (context.filterClause() != null)
        {
            function.FilterExpression =
                (Expression.IExpression)Visit(context.filterClause().whereClause().expression());
        }
    }

    /// <summary>
    /// 构建 MySQL GROUP_CONCAT 函数。对应上游 commit ff28f826。
    /// 语法：GROUP_CONCAT(DISTINCT? expressionList? orderByClause? (SEPARATOR expression)?)
    /// </summary>
    // CONVERT / TRY_CONVERT / SAFE_CONVERT 双风格转码函数

    public override object VisitTranscodingFunction(JSqlParserGrammar.TranscodingFunctionContext context)
    {
        var keyword = context.TRY_CONVERT() != null ? "TRY_CONVERT"
            : context.SAFE_CONVERT() != null ? "SAFE_CONVERT" : "CONVERT";

        var body = (TranscodingFunction)Visit(context.transcodingBody());
        body.Keyword = keyword;
        return body;
    }

    public override object VisitTranscodingTypeStyle(JSqlParserGrammar.TranscodingTypeStyleContext context)
    {
        return new TranscodingFunction
        {
            IsTranscodeStyle = false,
            ColDataType = context.dataType().GetText(),
            Expression = (Expression.IExpression)Visit(context.expression()),
            TranscodingName = context.LONG_VALUE()?.GetText(),
        };
    }

    public override object VisitTranscodingTranscodeStyle(JSqlParserGrammar.TranscodingTranscodeStyleContext context)
    {
        return new TranscodingFunction
        {
            IsTranscodeStyle = true,
            Expression = (Expression.IExpression)Visit(context.expression()),
            TranscodingName = context.transcodingName().GetText(),
        };
    }

    // JSON_OBJECT( [KEY] k (VALUE|:|,) v [FORMAT JSON] ... )

    public override object VisitJsonObjectFunction(JSqlParserGrammar.JsonObjectFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.OBJECT);

        foreach (var kvpCtx in context.jsonKeyValuePair())
        {
            func.KeyValuePairs.Add((JsonKeyValuePair)Visit(kvpCtx));
        }

        if (context.onNullClause() != null)
        {
            func.OnNull = context.onNullClause().ABSENT() != null
                ? JsonFunction.OnNullType.ABSENT
                : JsonFunction.OnNullType.NULL;
        }

        if (context.STRICT() != null)
        {
            func.Strict = true;
        }

        if (context.uniqueKeysClause() != null)
        {
            func.UniqueKeys = context.uniqueKeysClause().WITH() != null
                ? JsonFunction.UniqueKeysType.WITH
                : JsonFunction.UniqueKeysType.WITHOUT;
        }

        FillJsonReturning(func, context.jsonReturningClause());

        return func;
    }

    public override object VisitJsonKeyValuePair(JSqlParserGrammar.JsonKeyValuePairContext context)
    {
        var kvp = new JsonKeyValuePair { UsingKeyKeyword = context.KEY() != null };

        // key：S_CHAR_LITERAL 或 columnRef
        kvp.Key = context.S_CHAR_LITERAL(0) != null
            ? (object)new StringValue(context.S_CHAR_LITERAL(0).GetText())
            : Visit(context.columnRef(0));

        // 分隔符
        if (context.VALUE() != null)
        {
            kvp.Separator = JsonKeyValuePair.SeparatorKind.VALUE;
        }
        else if (context.DOUBLE_COLON() != null || context.COLON() != null || context.S_JDBC_NAMED_PARAM() != null)
        {
            kvp.Separator = JsonKeyValuePair.SeparatorKind.COLON;
        }
        else
        {
            kvp.Separator = JsonKeyValuePair.SeparatorKind.COMMA;
        }

        // 无空格冒号形式 key:bar：:bar 被词法分析为 S_JDBC_NAMED_PARAM，去前导冒号得到值
        if (context.S_JDBC_NAMED_PARAM() != null)
        {
            var raw = context.S_JDBC_NAMED_PARAM().GetText();
            kvp.Value = new Column { ColumnName = raw.Length > 1 ? raw[1..] : raw };
        }

        // value：分隔符存在时，取其后的 S_CHAR_LITERAL / columnRef / expression
        // 注意 key 可能是 S_CHAR_LITERAL 或 columnRef，value 的索引需根据 key 实际用法调整
        var charLits = context.S_CHAR_LITERAL();
        var colRefs = context.columnRef();
        if (charLits.Length > 1)
        {
            // key 是 S_CHAR_LITERAL(0)，value 是 S_CHAR_LITERAL(1)
            kvp.Value = new StringValue(charLits[1].GetText());
        }
        else if (colRefs.Length > 1)
        {
            // key 是 columnRef(0)，value 是 columnRef(1)
            kvp.Value = Visit(colRefs[1]);
        }
        else if (colRefs.Length == 1 && charLits.Length == 1)
        {
            // key 是 S_CHAR_LITERAL(0)，value 是 columnRef(0)
            kvp.Value = Visit(colRefs[0]);
        }
        else if (context.expression() != null)
        {
            kvp.Value = Visit(context.expression());
        }

        if (context.FORMAT() != null)
        {
            kvp.UsingFormatJson = true;
            if (context.ENCODING() != null)
            {
                kvp.Encoding = context.identifier().GetText();
            }
        }

        return kvp;
    }

    // JSON_ARRAY( expr [FORMAT JSON] ... )

    public override object VisitJsonArrayFunction(JSqlParserGrammar.JsonArrayFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.ARRAY);

        foreach (var elemCtx in context.jsonArrayElement())
        {
            var elem = new JsonFunctionExpression
            {
                Expression = (Expression.IExpression)Visit(elemCtx.expression())
            };
            if (elemCtx.FORMAT() != null)
            {
                elem.UsingFormatJson = true;
                if (elemCtx.ENCODING() != null)
                {
                    elem.Encoding = elemCtx.identifier().GetText();
                }
            }
            func.Expressions.Add(elem);
        }

        if (context.onNullClause() != null)
        {
            func.OnNull = context.onNullClause().ABSENT() != null
                ? JsonFunction.OnNullType.ABSENT
                : JsonFunction.OnNullType.NULL;
        }

        FillJsonReturning(func, context.jsonReturningClause());

        return func;
    }

    // JSON 函数输入表达式：expression [FORMAT JSON [ENCODING x]]

    public override object VisitJsonFunctionInput(JSqlParserGrammar.JsonFunctionInputContext context)
    {
        var input = new JsonFunctionExpression
        {
            Expression = (Expression.IExpression)Visit(context.expression())
        };
        if (context.FORMAT() != null)
        {
            input.UsingFormatJson = true;
            if (context.ENCODING() != null)
            {
                input.Encoding = context.identifier().GetText();
            }
        }
        return input;
    }

    // JSON_VALUE(input, path, ...)

    public override object VisitJsonValueFunction(JSqlParserGrammar.JsonValueFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.VALUE);
        func.InputExpression = (JsonFunctionExpression)Visit(context.jsonFunctionInput());
        // jsonFunctionInput 之后的第一个 expression 即 path
        func.JsonPathExpression = (Expression.IExpression)Visit(context.expression(0));
        FillJsonReturning(func, context.jsonReturningClause());

        // ON EMPTY / ON ERROR：根据 EMPTY_KW clause 是否存在判断 behavior 归属
        // grammar: (jsonValueBehavior ON EMPTY_KW)? (jsonValueBehavior ON ERROR)?
        var behaviors = context.jsonValueBehavior();
        bool hasEmpty = context.EMPTY_KW() != null;
        if (behaviors.Length == 1)
        {
            // 只有 1 个 behavior：归属 ON EMPTY 或 ON ERROR 之一
            if (hasEmpty)
                func.OnEmptyBehavior = ParseJsonValueBehavior(behaviors[0]);
            else
                func.OnErrorBehavior = ParseJsonValueBehavior(behaviors[0]);
        }
        else if (behaviors.Length >= 2)
        {
            func.OnEmptyBehavior = ParseJsonValueBehavior(behaviors[0]);
            func.OnErrorBehavior = ParseJsonValueBehavior(behaviors[1]);
        }
        return func;
    }

    // JSON_EXISTS(input, path, ...)

    public override object VisitJsonExistsFunction(JSqlParserGrammar.JsonExistsFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.EXISTS);
        func.InputExpression = (JsonFunctionExpression)Visit(context.jsonFunctionInput());
        func.JsonPathExpression = (Expression.IExpression)Visit(context.expression(0));

        if (context.jsonExistsBehavior() != null)
        {
            var b = context.jsonExistsBehavior();
            var type = b.TRUE() != null ? JsonFunction.OnResponseBehaviorType.TRUE
                : b.FALSE() != null ? JsonFunction.OnResponseBehaviorType.FALSE
                : b.UNKNOWN() != null ? JsonFunction.OnResponseBehaviorType.UNKNOWN
                : JsonFunction.OnResponseBehaviorType.ERROR;
            func.OnErrorBehavior = new JsonFunction.JsonOnResponseBehavior(type);
        }
        return func;
    }

    // JSON_QUERY(input, path, ...)

    public override object VisitJsonQueryFunction(JSqlParserGrammar.JsonQueryFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.QUERY);
        func.InputExpression = (JsonFunctionExpression)Visit(context.jsonFunctionInput());
        func.JsonPathExpression = (Expression.IExpression)Visit(context.expression(0));
        FillJsonReturning(func, context.jsonReturningClause());

        if (context.jsonWrapperClause() != null)
        {
            var w = context.jsonWrapperClause();
            func.Wrapper = w.WITHOUT() != null
                ? JsonFunction.WrapperType.WITHOUT
                : JsonFunction.WrapperType.WITH;
            if (w.CONDITIONAL() != null) func.WrapperModeValue = JsonFunction.WrapperMode.CONDITIONAL;
            if (w.UNCONDITIONAL() != null) func.WrapperModeValue = JsonFunction.WrapperMode.UNCONDITIONAL;
            if (w.ARRAY() != null) func.WrapperArray = true;
        }

        if (context.jsonQuotesClause() != null)
        {
            var q = context.jsonQuotesClause();
            func.Quotes = q.KEEP() != null ? JsonFunction.QuotesType.KEEP : JsonFunction.QuotesType.OMIT;
            if (q.SCALAR() != null) func.QuotesOnScalarString = true;
        }

        // ON EMPTY / ON ERROR
        var behaviors = context.jsonQueryBehavior();
        bool hasEmpty = context.EMPTY_KW() != null;
        if (behaviors.Length == 1)
        {
            if (hasEmpty) func.OnEmptyBehavior = ParseJsonQueryBehavior(behaviors[0]);
            else func.OnErrorBehavior = ParseJsonQueryBehavior(behaviors[0]);
        }
        else if (behaviors.Length >= 2)
        {
            func.OnEmptyBehavior = ParseJsonQueryBehavior(behaviors[0]);
            func.OnErrorBehavior = ParseJsonQueryBehavior(behaviors[1]);
        }

        // Legacy 额外 path 参数（JSON_QUERY(input, path1, path2...)），仅在无 PASSING 时收集
        // context.expression() 含首个 path（index 0）+ 额外 path（index 1+）
        if (func.PassingExpressions.Count == 0)
        {
            var allExprs = context.expression();
            for (int i = 1; i < allExprs.Length; i++)
            {
                var pathExpression = (Expression.IExpression?)Visit(allExprs[i]);
                func.AdditionalQueryPathArguments.Add(pathExpression?.ToString() ?? string.Empty);
            }
        }
        return func;
    }

    private JsonFunction.JsonOnResponseBehavior ParseJsonQueryBehavior(JSqlParserGrammar.JsonQueryBehaviorContext b)
    {
        if (b.DEFAULT() != null)
        {
            return new JsonFunction.JsonOnResponseBehavior(
                JsonFunction.OnResponseBehaviorType.DEFAULT,
                (Expression.IExpression)Visit(b.expression()));
        }
        if (b.TRUE() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.TRUE);
        if (b.FALSE() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.FALSE);
        if (b.ARRAY() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY_ARRAY);
        if (b.OBJECT() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY_OBJECT);
        if (b.EMPTY_KW() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY);
        if (b.ERROR() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.ERROR);
        return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.NULL);
    }

    // JSON_OBJECTAGG([KEY] key (VALUE|:|,) value [FORMAT JSON] [ON NULL] [UNIQUE KEYS])

    public override object VisitJsonObjectAggFunction(JSqlParserGrammar.JsonObjectAggFunctionContext context)
    {
        var func = new JsonAggregateFunction
        {
            AggregateFunctionType = JsonAggregateFunction.AggregateType.OBJECT,
            Name = "JSON_OBJECTAGG",
            UsingKeyKeyword = context.KEY() != null
        };

        // key
        func.Key = context.S_CHAR_LITERAL() != null
            ? (object)new StringValue(context.S_CHAR_LITERAL().GetText())
            : Visit(context.columnRef());

        // 分隔符
        if (context.VALUE() != null)
        {
            func.UsingValueSeparator = true;
            func.UsingValueKeyword = true;
        }
        else if (context.DOUBLE_COLON() != null || context.COLON() != null)
        {
            func.UsingValueSeparator = false;
        }
        else if (context.COMMA() != null)
        {
            // COMMA 分隔（MySQL 风格 key,value），对齐上游 MYSQL_OBJECT
            func.UsingValueSeparator = false;
            func.UseCommaSeparator = true;
        }

        // value
        func.Value = (Expression.IExpression)Visit(context.expression());

        if (context.FORMAT() != null) func.UsingFormatJson = true;

        if (context.onNullClause() != null)
        {
            func.OnNull = context.onNullClause().ABSENT() != null
                ? JsonFunction.OnNullType.ABSENT
                : JsonFunction.OnNullType.NULL;
        }

        if (context.uniqueKeysClause() != null)
        {
            func.UniqueKeys = context.uniqueKeysClause().WITH() != null
                ? JsonFunction.UniqueKeysType.WITH
                : JsonFunction.UniqueKeysType.WITHOUT;
        }

        return func;
    }

    // JSON_ARRAYAGG(expr [FORMAT JSON] [ORDER BY ...] [ON NULL])

    public override object VisitJsonArrayAggFunction(JSqlParserGrammar.JsonArrayAggFunctionContext context)
    {
        var func = new JsonAggregateFunction
        {
            AggregateFunctionType = JsonAggregateFunction.AggregateType.ARRAY,
            Name = "JSON_ARRAYAGG",
            AggregateExpression = (Expression.IExpression)Visit(context.expression())
        };

        if (context.FORMAT() != null) func.UsingFormatJson = true;

        if (context.orderByClause() != null)
        {
            func.OrderByElements = (List<OrderByElement>)Visit(context.orderByClause());
        }

        if (context.onNullClause() != null)
        {
            func.OnNull = context.onNullClause().ABSENT() != null
                ? JsonFunction.OnNullType.ABSENT
                : JsonFunction.OnNullType.NULL;
        }

        return func;
    }

    private JsonFunction.JsonOnResponseBehavior ParseJsonValueBehavior(JSqlParserGrammar.JsonValueBehaviorContext b)
    {
        if (b.DEFAULT() != null)
        {
            return new JsonFunction.JsonOnResponseBehavior(
                JsonFunction.OnResponseBehaviorType.DEFAULT,
                (Expression.IExpression)Visit(b.expression()));
        }
        if (b.EMPTY_KW() != null)
        {
            // EMPTY 行为输出 "EMPTY "（带尾空格，上游 JsonOnResponseBehavior 特性）
            return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY);
        }
        if (b.ERROR() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.ERROR);
        return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.NULL);
    }

    private void FillJsonReturning(JsonFunction func, JSqlParserGrammar.JsonReturningClauseContext? ret)
    {
        if (ret == null) return;
        func.ReturningType = ret.dataType().GetText();
        if (ret.FORMAT() != null)
        {
            func.ReturningFormatJson = true;
            if (ret.ENCODING() != null)
            {
                func.ReturningEncoding = ret.identifier().GetText();
            }
        }
    }

    public override object VisitGroupConcatFunction(JSqlParserGrammar.GroupConcatFunctionContext context)
    {
        var func = new Function { Name = "GROUP_CONCAT" };

        if (context.DISTINCT() != null)
        {
            func.Distinct = true;
        }

        if (context.expressionList() != null)
        {
            var parameters = new ExpressionList { Expressions = new List<Expression.IExpression>() };
            foreach (var expr in context.expressionList().expression())
            {
                parameters.Expressions.Add((Expression.IExpression)Visit(expr));
            }
            func.Parameters = parameters;
        }

        if (context.orderByClause() != null)
        {
            func.OrderByElements = (List<OrderByElement>)Visit(context.orderByClause());
        }

        if (context.SEPARATOR() != null)
        {
            func.Separator = (Expression.IExpression)Visit(context.expression());
        }

        if (context.filterClause() != null)
        {
            func.FilterExpression =
                (Expression.IExpression)Visit(context.filterClause().whereClause().expression());
        }

        return func;
    }

    private void ApplyFunctionClauses(JSqlParserGrammar.FunctionExprContext context, AnalyticExpression analytic)
    {
        if (context.withinGroupClause() != null)
        {
            analytic.WithinGroupOrderByElements =
                (List<OrderByElement>)Visit(context.withinGroupClause().orderByClause());
        }

        if (context.filterClause() != null)
        {
            analytic.FilterExpression =
                (Expression.IExpression)Visit(context.filterClause().whereClause().expression());
        }
    }
}
