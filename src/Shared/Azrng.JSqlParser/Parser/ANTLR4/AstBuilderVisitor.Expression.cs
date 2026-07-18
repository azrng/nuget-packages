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
/// AstBuilderVisitor 的 Expression 分部：表达式层级：逻辑、谓词、算术、字面量、CASE/CAST、列/表/数组/结构等。
/// </summary>
public partial class AstBuilderVisitor
{
    public override object VisitExpressionEntry(JSqlParserGrammar.ExpressionEntryContext context)
    {
        return Visit(context.expression());
    }

    public override object VisitExpression(JSqlParserGrammar.ExpressionContext context)
    {
        return Visit(context.orExpression());
    }

    public override object VisitOrExpression(JSqlParserGrammar.OrExpressionContext context)
    {
        var andExprs = context.andExpression();
        if (andExprs.Length == 1)
        {
            return Visit(andExprs[0]);
        }

        Expression.IExpression result = (Expression.IExpression)Visit(andExprs[0]);
        for (int i = 1; i < andExprs.Length; i++)
        {
            result = new OrExpression
            {
                LeftExpression = result,
                RightExpression = (Expression.IExpression)Visit(andExprs[i])
            };
        }
        return result;
    }

    public override object VisitAndExpression(JSqlParserGrammar.AndExpressionContext context)
    {
        var notExprs = context.notExpression();
        if (notExprs.Length == 1)
        {
            return Visit(notExprs[0]);
        }

        Expression.IExpression result = (Expression.IExpression)Visit(notExprs[0]);
        for (int i = 1; i < notExprs.Length; i++)
        {
            result = new AndExpression
            {
                LeftExpression = result,
                RightExpression = (Expression.IExpression)Visit(notExprs[i])
            };
        }
        return result;
    }

    public override object VisitNotExpression(JSqlParserGrammar.NotExpressionContext context)
    {
        if (context.NOT() != null)
        {
            return new NotExpression
            {
                Expression = (Expression.IExpression)Visit(context.notExpression())
            };
        }
        return Visit(context.predicate());
    }

    public override object VisitPredicate(JSqlParserGrammar.PredicateContext context)
    {
        if (context.EXISTS() != null)
        {
            var exists = new ExistsExpression();
            exists.RightExpression = (Expression.IExpression)Visit(context.selectStatement());
            return exists;
        }

        var concat = (Expression.IExpression)Visit(context.concatenationExpr());

        if (context.predicateSuffix() == null)
        {
            return concat;
        }

        var suffix = context.predicateSuffix();

        if (suffix.comparisonOperator() != null)
        {
            var op = suffix.comparisonOperator();

            // = ANY/ALL/SOME (subquery) 形式
            if (suffix.ANY() != null || suffix.SOME() != null || suffix.ALL() != null)
            {
                var anyType = suffix.ALL() != null ? AnyType.All
                    : suffix.SOME() != null ? AnyType.Some : AnyType.Any;
                var select = (Select)Visit(suffix.selectStatement());
                // 包装成比较运算符 + ANY/ALL/SOME
                var anyCompare = new AnyComparisonExpression(anyType, select);
                Expression.IExpression result = anyCompare;
                if (op.EQUALS() != null) return new EqualsTo { LeftExpression = concat, RightExpression = result };
                if (op.NOT_EQUALS() != null || op.NOT_EQUALS2() != null || op.NOT_EQUALS3() != null)
                    return new NotEqualsTo { LeftExpression = concat, RightExpression = result };
                if (op.GREATER_THAN() != null) return new GreaterThan { LeftExpression = concat, RightExpression = result };
                if (op.GREATER_THAN_EQUALS() != null) return new GreaterThanEquals { LeftExpression = concat, RightExpression = result };
                if (op.MINOR_THAN() != null) return new MinorThan { LeftExpression = concat, RightExpression = result };
                if (op.MINOR_THAN_EQUALS() != null) return new MinorThanEquals { LeftExpression = concat, RightExpression = result };
                // 理论不可达：comparisonOperator production 限定为 14 个 token，全部在上面分支覆盖。
                // 此前兜底为 EqualsTo 会静默把未知 token 当作 =（正是 ~ / !~ bug 的同型根因）。
                // 改抛异常让 grammar 漏列 token 时立即暴露，避免静默误归类。
                throw new JSqlParserException(
                    $"Unreachable: comparisonOperator with ANY/ALL/SOME 未匹配任何已知 token。grammar comparisonOperator production 可能漏列。原操作符文本：{op.GetText()}");
            }

            Expression.IExpression right = (Expression.IExpression)Visit(suffix.concatenationExpr(0));

            if (op.GEOMETRY_DISTANCE() != null)
                return new GeometryDistance("<->") { LeftExpression = concat, RightExpression = right };
            if (op.GEOMETRY_DISTANCE_HASH() != null)
                return new GeometryDistance("<#>") { LeftExpression = concat, RightExpression = right };
            if (op.EQUALS() != null) return new EqualsTo { LeftExpression = concat, RightExpression = right };
            if (op.NOT_EQUALS() != null || op.NOT_EQUALS2() != null || op.NOT_EQUALS3() != null)
                return new NotEqualsTo { LeftExpression = concat, RightExpression = right };
            if (op.GREATER_THAN() != null) return new GreaterThan { LeftExpression = concat, RightExpression = right };
            if (op.GREATER_THAN_EQUALS() != null) return new GreaterThanEquals { LeftExpression = concat, RightExpression = right };
            if (op.MINOR_THAN() != null) return new MinorThan { LeftExpression = concat, RightExpression = right };
            if (op.MINOR_THAN_EQUALS() != null) return new MinorThanEquals { LeftExpression = concat, RightExpression = right };

            // PostgreSQL 正则匹配运算符 ~ / ~* / !~ / !~*：归一到 RegExpMatchOperator + 枚举，
            // 对齐上游 JSqlParserCC.jjt:6834-6837。否定语义内嵌在 OperatorType 枚举成员
            //（NotMatchCaseSensitive 等），不依赖 Not 前缀——PG 否定写法是符号前置 !~。
            // 修复前这些 token 落到下面默认分支被错误地建成 EqualsTo（=）。
            if (op.TILDE() != null || op.TILDE_STAR() != null || op.NOT_TILDE() != null || op.NOT_TILDE_STAR() != null)
            {
                var opType = op.TILDE() != null ? RegExpMatchOperatorType.MatchCaseSensitive
                    : op.TILDE_STAR() != null ? RegExpMatchOperatorType.MatchCaseInsensitive
                    : op.NOT_TILDE() != null ? RegExpMatchOperatorType.NotMatchCaseSensitive
                    : RegExpMatchOperatorType.NotMatchCaseInsensitive;
                return new RegExpMatchOperator(opType)
                {
                    LeftExpression = concat,
                    RightExpression = right
                };
            }

            // PostgreSQL 全文匹配运算符 @@ / @@@（tsvector @@ tsquery），
            // 复用既有 Matches 类型（Operators/Relational/Matches.cs，符号 @@）。
            if (op.AT_AT() != null || op.AT_AT_AT() != null)
            {
                return new Matches { LeftExpression = concat, RightExpression = right };
            }

            // 理论不可达：comparisonOperator production 限定为 14 个 token，全部在上面分支覆盖。
            // 此前兜底为 EqualsTo 会静默把未知 token 当作 =（正是 ~ / !~ bug 的同型根因）。
            // 改抛异常让 grammar 漏列 token 时立即暴露，避免静默误归类。
            throw new JSqlParserException(
                $"Unreachable: comparisonOperator 未匹配任何已知 token。grammar comparisonOperator production 可能漏列。原操作符文本：{op.GetText()}");
        }

        if (suffix.IN() != null)
        {
            var inExpr = new InExpression { LeftExpression = concat };
            if (suffix.selectStatement() != null)
            {
                inExpr.RightExpression = (Expression.IExpression)Visit(suffix.selectStatement());
            }
            else if (suffix.expressionList() != null)
            {
                inExpr.RightExpression = (Expression.IExpression)Visit(suffix.expressionList());
            }
            if (suffix.NOT() != null) inExpr.Not = true;
            // ClickHouse GLOBAL IN / GLOBAL NOT IN：对齐上游 InExpression.global
            if (suffix.GLOBAL() != null) inExpr.Global = true;
            return inExpr;
        }

        if (suffix.BETWEEN() != null)
        {
            var between = new Between
            {
                LeftExpression = concat,
                BetweenExpressionStart = (Expression.IExpression)Visit(suffix.concatenationExpr(0)),
                BetweenExpressionEnd = (Expression.IExpression)Visit(suffix.concatenationExpr(1))
            };
            if (suffix.NOT() != null) between.Not = true;
            if (suffix.SYMMETRIC() != null) between.UsingSymmetric = true;
            else if (suffix.ASYMMETRIC() != null) between.UsingAsymmetric = true;
            return between;
        }

        // 关键字形式的模式匹配（LIKE/ILIKE/RLIKE/REGEXP/REGEXP_LIKE/MATCH_*/SIMILAR TO）
        // 统一建成 LikeExpression，通过 KeyWord 区分——对齐上游 LikeExpression 模型。
        // 此前 REGEXP/RLIKE 错误地建成 RegExpMatchOperator、SIMILAR TO 建成独立 SimilarToExpression，已合并。
        if (suffix.LIKE() != null || suffix.ILIKE() != null || suffix.RLIKE() != null
            || suffix.REGEXP() != null || suffix.REGEXP_LIKE() != null
            || suffix.MATCH_ANY() != null || suffix.MATCH_ALL() != null
            || suffix.MATCH_PHRASE() != null || suffix.MATCH_PHRASE_PREFIX() != null || suffix.MATCH_REGEXP() != null
            || suffix.SIMILAR() != null)
        {
            var like = new LikeExpression
            {
                LeftExpression = concat,
                RightExpression = (Expression.IExpression)Visit(suffix.concatenationExpr(0))
            };
            // 关键字映射到 KeyWord 枚举（对齐上游 LikeExpression.KeyWord.from(token.image)）
            like.LikeKeyWord =
                suffix.LIKE() != null ? LikeExpression.KeyWord.Like
                : suffix.ILIKE() != null ? LikeExpression.KeyWord.Ilike
                : suffix.RLIKE() != null ? LikeExpression.KeyWord.Rlike
                : suffix.REGEXP() != null ? LikeExpression.KeyWord.Regexp
                : suffix.REGEXP_LIKE() != null ? LikeExpression.KeyWord.RegexpLike
                : suffix.MATCH_ANY() != null ? LikeExpression.KeyWord.MatchAny
                : suffix.MATCH_ALL() != null ? LikeExpression.KeyWord.MatchAll
                : suffix.MATCH_PHRASE() != null ? LikeExpression.KeyWord.MatchPhrase
                : suffix.MATCH_PHRASE_PREFIX() != null ? LikeExpression.KeyWord.MatchPhrasePrefix
                : suffix.MATCH_REGEXP() != null ? LikeExpression.KeyWord.MatchRegexp
                : LikeExpression.KeyWord.SimilarTo;
            if (suffix.NOT() != null) like.Not = true;
            // MySQL BINARY 前缀（x LIKE BINARY 'A' 大小写敏感匹配），对齐上游 useBinary
            if (suffix.BINARY() != null) like.UseBinary = true;
            // PostgreSQL 数组量词（col LIKE ANY (ARRAY[...]) / LIKE ALL (...)）
            if (suffix.ANY() != null) like.LikeQuantifier = AnyType.Any;
            else if (suffix.ALL() != null) like.LikeQuantifier = AnyType.All;
            // ESCAPE 子句（grammar 1238/1239 行已支持，对齐上游 escapeExpression）
            if (suffix.ESCAPE() != null)
            {
                like.Escape = (Expression.IExpression)Visit(suffix.concatenationExpr(1));
            }
            return like;
        }

        if (suffix.IS() != null)
        {
            if (suffix.DISTINCT() != null)
            {
                var isDistinct = new IsDistinctExpression
                {
                    LeftExpression = concat,
                    RightExpression = (Expression.IExpression)Visit(suffix.concatenationExpr(0))
                };
                if (suffix.NOT() != null) isDistinct.Not = true;
                return isDistinct;
            }

            if (suffix.TRUE() != null)
            {
                var isBool = new IsBooleanExpression { LeftExpression = concat, IsTrue = true };
                if (suffix.NOT() != null) isBool.Not = true;
                return isBool;
            }

            if (suffix.FALSE() != null)
            {
                var isBool = new IsBooleanExpression { LeftExpression = concat, IsTrue = false };
                if (suffix.NOT() != null) isBool.Not = true;
                return isBool;
            }

            if (suffix.UNKNOWN() != null)
            {
                var isUnknown = new IsUnknownExpression { LeftExpression = concat };
                if (suffix.NOT() != null) isUnknown.Not = true;
                return isUnknown;
            }

            var isNull = new IsNullExpression { LeftExpression = concat };
            if (suffix.NOT() != null) isNull.Not = true;
            return isNull;
        }

        if (suffix.ISNULL() != null)
        {
            // PostgreSQL 简写 x ISNULL：UseIsNull=true，ToString 输出 x ISNULL（保 round-trip）
            return new IsNullExpression { LeftExpression = concat, UseIsNull = true };
        }

        if (suffix.NOTNULL() != null)
        {
            // PostgreSQL 简写 x NOTNULL：UseNotNull=true，ToString 输出 x NOTNULL（保 round-trip）
            return new IsNullExpression { LeftExpression = concat, UseNotNull = true };
        }

        if (suffix.EXCLUDES() != null)
        {
            return new ExcludesExpression(concat, (Expression.IExpression)Visit(suffix.expressionList()));
        }

        if (suffix.INCLUDES() != null)
        {
            return new IncludesExpression(concat, (Expression.IExpression)Visit(suffix.expressionList()));
        }

        if (suffix.MEMBER() != null)
        {
            var memberOf = new MemberOfExpression
            {
                LeftExpression = concat,
                RightExpression = (Expression.IExpression)Visit(suffix.concatenationExpr(0))
            };
            if (suffix.NOT() != null) memberOf.Not = true;
            return memberOf;
        }

        if (suffix.OVERLAPS() != null)
        {
            // 左侧：当前 Azrng grammar 不支持括号列表，按单元素 ExpressionList 包装
            var left = new ExpressionList { Expressions = new() { concat } };
            var right = (Expression.IExpression)Visit(suffix.concatenationExpr(0));
            var rightList = new ExpressionList { Expressions = new() { right } };
            return new OverlapsCondition { LeftExpression = left, RightExpression = rightList };
        }

        return concat;
    }

    public override object VisitConcatenationExpr(JSqlParserGrammar.ConcatenationExprContext context)
    {
        var additiveExprs = context.additiveExpr();
        Expression.IExpression result;
        if (additiveExprs.Length == 1)
        {
            result = (Expression.IExpression)Visit(additiveExprs[0]);
        }
        else
        {
            result = (Expression.IExpression)Visit(additiveExprs[0]);
            for (int i = 1; i < additiveExprs.Length; i++)
            {
                result = new Concat
                {
                    LeftExpression = result,
                    RightExpression = (Expression.IExpression)Visit(additiveExprs[i])
                };
            }
        }

        // COLLATE 后缀（仅当存在且非 ORDER BY 上下文已消化时）
        if (context.COLLATE() != null)
        {
            var collateName = context.S_CHAR_LITERAL()?.GetText() ?? context.identifier().GetText();
            return new CollateExpression(result, collateName);
        }

        return result;
    }

    public override object VisitAdditiveExpr(JSqlParserGrammar.AdditiveExprContext context)
    {
        var multiplicativeExprs = context.multiplicativeExpr();
        if (multiplicativeExprs.Length == 1)
        {
            return Visit(multiplicativeExprs[0]);
        }

        Expression.IExpression result = (Expression.IExpression)Visit(multiplicativeExprs[0]);
        for (int i = 1; i < multiplicativeExprs.Length; i++)
        {
            var op = context.GetChild(2 * i - 1);
            Expression.IExpression right = (Expression.IExpression)Visit(multiplicativeExprs[i]);

            if (op is ITerminalNode terminal)
            {
                if (terminal.Symbol.Type == JSqlParserGrammarLexer.PLUS)
                {
                    result = new Addition
                    {
                        LeftExpression = result,
                        RightExpression = right
                    };
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.MINUS)
                {
                    result = new Subtraction
                    {
                        LeftExpression = result,
                        RightExpression = right
                    };
                }
            }
        }
        return result;
    }

    public override object VisitMultiplicativeExpr(JSqlParserGrammar.MultiplicativeExprContext context)
    {
        var unaryExprs = context.unaryExpr();
        if (unaryExprs.Length == 1)
        {
            return Visit(unaryExprs[0]);
        }

        Expression.IExpression result = (Expression.IExpression)Visit(unaryExprs[0]);
        for (int i = 1; i < unaryExprs.Length; i++)
        {
            var op = context.GetChild(2 * i - 1);
            Expression.IExpression right = (Expression.IExpression)Visit(unaryExprs[i]);

            if (op is ITerminalNode terminal)
            {
                if (terminal.Symbol.Type == JSqlParserGrammarLexer.MULTIPLY)
                {
                    result = new Multiplication
                    {
                        LeftExpression = result,
                        RightExpression = right
                    };
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.DIVIDE)
                {
                    result = new Division
                    {
                        LeftExpression = result,
                        RightExpression = right
                    };
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.MODULO)
                {
                    result = new Modulo
                    {
                        LeftExpression = result,
                        RightExpression = right
                    };
                }
            }
        }
        return result;
    }

    public override object VisitUnaryExpr(JSqlParserGrammar.UnaryExprContext context)
    {
        if (context.postfixExpr() != null)
        {
            return Visit(context.postfixExpr());
        }

        var expr = (Expression.IExpression)Visit(context.unaryExpr());
        if (context.MINUS() != null)
        {
            return new SignedExpression
            {
                Sign = '-',
                Expression = expr
            };
        }

        return expr;
    }

    public override object VisitPostfixExpr(JSqlParserGrammar.PostfixExprContext context)
    {
        var expr = (Expression.IExpression)Visit(context.primaryExpr());

        // 按出现顺序处理后缀操作符：::colDataType（cast）、.identifier（字段访问）、
        // COLLATE collation、AT TIME ZONE expr。
        // ANTLR 的 postfixExpr 文法将这些操作符以 * 循环混合，需顺序遍历子节点。
        // 注：grammar 用 colDataType（含数组维度 [] 与 TIME ZONE 后缀），支持 ::text[] / ::character varying
        var colDataTypes = context.colDataType();
        int colDataTypeIdx = 0;
        var identifiers = context.identifier();
        int identifierIdx = 0;
        // postfixExpr 内部的 expression 子节点（用于 AT TIME ZONE）
        var subExpressions = context.expression();
        int subExprIdx = 0;
        // 跳过 OPENING_PAREN ( ... ) 的子表达式（与 AT TIME ZONE 共用 expression 规则）
        // 通过遍历过程动态判断当前 expression 是属于 AT TIME ZONE 还是函数调用
        bool expectingTimeZoneExpr = false;
        bool inBracket = false;
        // 用于范围表达式的临时存储
        Expression.IExpression? pendingStartIndex = null;

        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is ITerminalNode terminal)
            {
                if (terminal.Symbol.Type == JSqlParserGrammarLexer.DOUBLE_COLON && colDataTypeIdx < colDataTypes.Length)
                {
                    expr = new CastExpression
                    {
                        Expression = expr,
                        DataType = colDataTypes[colDataTypeIdx].GetText(),
                        UseCastKeyword = false
                    };
                    colDataTypeIdx++;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.DOT && identifierIdx < identifiers.Length)
                {
                    // PostgreSQL 复合类型字段访问：(expr).field 或多层 (expr).field1.field2
                    expr = new RowGetExpression(expr, identifiers[identifierIdx].GetText());
                    identifierIdx++;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.AT)
                {
                    expectingTimeZoneExpr = true;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.LBRACKET)
                {
                    inBracket = true;
                    pendingStartIndex = null;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.COLON && inBracket)
                {
                    // 范围表达式：[start:end]，start 已存于 pendingStartIndex
                    // 不做处理，等待结束 expression
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.RBRACKET && inBracket)
                {
                    inBracket = false;
                    pendingStartIndex = null;
                }
            }
            else if (child is JSqlParserGrammar.ExpressionContext)
            {
                if (expectingTimeZoneExpr && subExprIdx < subExpressions.Length)
                {
                    expr = new TimezoneExpression
                    {
                        LeftExpression = expr,
                        TimeZoneExpression = (Expression.IExpression)Visit(child)
                    };
                    subExprIdx++;
                    expectingTimeZoneExpr = false;
                }
                else if (inBracket && subExprIdx < subExpressions.Length)
                {
                    var idxExpr = (Expression.IExpression)Visit(child);
                    subExprIdx++;

                    // 检查下一个非终结符是否为 COLON（范围表达式）
                    int nextIdx = i + 1;
                    while (nextIdx < context.ChildCount && context.GetChild(nextIdx) is not ITerminalNode)
                        nextIdx++;
                    bool nextIsColon = nextIdx < context.ChildCount
                        && context.GetChild(nextIdx) is ITerminalNode nextTerminal
                        && nextTerminal.Symbol.Type == JSqlParserGrammarLexer.COLON;

                    if (nextIsColon && pendingStartIndex == null)
                    {
                        // 当前是 range 的 start
                        pendingStartIndex = idxExpr;
                    }
                    else if (pendingStartIndex != null)
                    {
                        // 当前是 range 的 end
                        expr = new ArrayExpression
                        {
                            ObjExpression = expr,
                            StartIndexExpression = pendingStartIndex,
                            StopIndexExpression = idxExpr
                        };
                        pendingStartIndex = null;
                    }
                    else
                    {
                        // 单索引
                        expr = new ArrayExpression
                        {
                            ObjExpression = expr,
                            IndexExpression = idxExpr
                        };
                    }
                }
            }
        }

        return expr;
    }

    public override object VisitPrimaryExpr(JSqlParserGrammar.PrimaryExprContext context)
    {
        if (context.literal() != null) return Visit(context.literal());
        if (context.parameter() != null) return Visit(context.parameter());
        if (context.caseExpr() != null) return Visit(context.caseExpr());
        if (context.castExpr() != null) return Visit(context.castExpr());
        if (context.extractExpr() != null) return Visit(context.extractExpr());
        if (context.intervalExpr() != null) return Visit(context.intervalExpr());
        if (context.functionExpr() != null) return Visit(context.functionExpr());
        if (context.subSelect() != null) return Visit(context.subSelect());
        if (context.structType() != null) return Visit(context.structType());
        if (context.lambdaExpression() != null) return Visit(context.lambdaExpression());
        if (context.connectByPriorOperator() != null) return Visit(context.connectByPriorOperator());
        if (context.connectByRootOperator() != null) return Visit(context.connectByRootOperator());
        if (context.keyExpression() != null) return Visit(context.keyExpression());
        if (context.fullTextSearch() != null) return Visit(context.fullTextSearch());
        if (context.namedFunctionParameter() != null) return Visit(context.namedFunctionParameter());
        if (context.trimFunction() != null) return Visit(context.trimFunction());
        if (context.arrayConstructor() != null) return Visit(context.arrayConstructor());
        if (context.rowConstructor() != null) return Visit(context.rowConstructor());
        if (context.timeKeyExpression() != null) return Visit(context.timeKeyExpression());
        if (context.columnRef() != null) return Visit(context.columnRef());
        if (context.MULTIPLY() != null) return new AllColumns();

        if (context.OPENING_PAREN() != null && context.expression() != null)
        {
            return new Parenthesis
            {
                Expression = (Expression.IExpression)Visit(context.expression())
            };
        }

        return new NullValue();
    }

    public override object VisitKeyExpression(JSqlParserGrammar.KeyExpressionContext context)
    {
        var inner = (Expression.IExpression)Visit(context.columnRef());
        return new KeyExpression(inner);
    }

    // Oracle/PostgreSQL 命名函数参数：name => expr 或 name := expr

    public override object VisitNamedFunctionParameter(JSqlParserGrammar.NamedFunctionParameterContext context)
    {
        var name = context.identifier().GetText();
        var expr = (Expression.IExpression)Visit(context.expression());
        // ARROW (=>) 为 Oracle 形式，ASSIGN (:=) 为 PostgreSQL 形式
        if (context.ARROW() != null)
        {
            return new OracleNamedFunctionParameter(name, expr);
        }
        return new PostgresNamedFunctionParameter(name, expr);
    }

    // 数组构造器：ARRAY[1, 2, 3] 或 [1, 2, 3]

    public override object VisitArrayConstructor(JSqlParserGrammar.ArrayConstructorContext context)
    {
        ExpressionList? exprList = null;
        if (context.arrayElementList() != null)
        {
            exprList = new ExpressionList { Expressions = new List<Expression.IExpression>() };
            foreach (var elem in context.arrayElementList().arrayElement())
            {
                exprList.Expressions.Add((Expression.IExpression)Visit(elem));
            }
        }
        return new ArrayConstructor(exprList, arrayKeyword: context.ARRAY() != null);
    }

    public override object VisitArrayElement(JSqlParserGrammar.ArrayElementContext context)
    {
        var start = (Expression.IExpression)Visit(context.expression(0));
        if (context.COLON() != null)
        {
            var end = (Expression.IExpression)Visit(context.expression(1));
            return new RangeExpression(start, end);
        }
        return start;
    }

    // 时间关键字表达式：CURRENT_DATE / CURRENT_TIMESTAMP 等

    public override object VisitTimeKeyExpression(JSqlParserGrammar.TimeKeyExpressionContext context)
    {
        // 取原始文本（保留大小写），与上游 TimeKeyExpression 行为一致
        return new TimeKeyExpression(context.GetText());
    }

    // 行构造器：ROW(1, 2, 3)

    public override object VisitRowConstructor(JSqlParserGrammar.RowConstructorContext context)
    {
        var exprList = new ExpressionList { Expressions = new List<Expression.IExpression>() };
        foreach (var expr in context.expressionList().expression())
        {
            exprList.Expressions.Add((Expression.IExpression)Visit(expr));
        }
        return new RowConstructor("ROW", exprList);
    }

    // TRIM([LEADING|TRAILING|BOTH] [chars] [FROM] str) 或 TRIM(str)

    public override object VisitSqlServerHints(JSqlParserGrammar.SqlServerHintsContext context)
    {
        var hints = new SQLServerHints();
        foreach (var hintCtx in context.sqlServerHint())
        {
            if (hintCtx.NOLOCK() != null)
            {
                hints.NoLock = true;
            }
            else if (hintCtx.INDEX() != null && hintCtx.identifier() != null)
            {
                hints.IndexName = hintCtx.identifier().GetText();
            }
        }
        return hints;
    }

    // MySQL 索引提示：USE|IGNORE|FORCE INDEX|KEY (idx1, ...)

    public override object VisitMySqlIndexHint(JSqlParserGrammar.MySqlIndexHintContext context)
    {
        var action = context.USE() != null ? "USE"
            : context.IGNORE() != null ? "IGNORE"
            : context.FORCE() != null ? "FORCE" : "";
        var qualifier = context.INDEX() != null ? "INDEX"
            : context.KEY() != null ? "KEY" : "INDEX";
        var names = context.identifier().Select(id => id.GetText()).ToList();

        // MySQL USE/FORCE/IGNORE INDEX FOR JOIN|ORDER BY|GROUP BY（对齐上游 forClause）
        string? forClause = null;
        if (context.FOR() != null)
        {
            if (context.JOIN() != null) forClause = "FOR JOIN";
            else if (context.ORDER() != null) forClause = "FOR ORDER BY";
            else if (context.GROUP() != null) forClause = "FOR GROUP BY";
        }

        return new MySQLIndexHint(action.ToUpperInvariant(), qualifier.ToUpperInvariant(), names)
        {
            ForClause = forClause
        };
    }

    public override object VisitFullTextSearch(JSqlParserGrammar.FullTextSearchContext context)
    {
        // 列改为结构化 Column（对齐上游 ExpressionList<Column>），保留表名前缀等元信息
        var columns = new List<Column>();
        foreach (var colCtx in context.columnRef())
        {
            columns.Add((Column)Visit(colCtx));
        }

        var fts = new FullTextSearch
        {
            MatchColumns = columns,
            MatchExpression = (Expression.IExpression)Visit(context.expression())
        };

        if (context.searchModifier() != null)
        {
            // 按 token 顺序用空格拼接修饰符文本（如 "IN BOOLEAN MODE"）
            var modifierCtx = context.searchModifier();
            fts.SearchModifier = string.Join(' ', modifierCtx.children
                .OfType<ITerminalNode>().Select(t => t.GetText()));
        }

        return fts;
    }

    public override object VisitLiteral(JSqlParserGrammar.LiteralContext context)
    {
        if (context.LONG_VALUE() != null)
            return new LongValue(ParseLong(context.LONG_VALUE().GetText()));
        if (context.S_DOUBLE() != null)
            return new DoubleValue(ParseDouble(context.S_DOUBLE().GetText()));
        if (context.S_CHAR_LITERAL() != null)
        {
            // S_CHAR_LITERAL 可能含可选前缀（N/E/U/R/B/RB/_utf8），交给 StringValue 构造函数识别
            var text = context.S_CHAR_LITERAL().GetText();
            return new StringValue(text);
        }
        if (context.S_ORACLE_Q_STRING() != null)
        {
            // Oracle q'...{...}...' 自定义分隔引号
            var text = context.S_ORACLE_Q_STRING().GetText();
            return new StringValue(text);
        }
        if (context.S_DOLLAR_QUOTED_STRING() != null)
        {
            // PostgreSQL dollar-quoted string: $$...$$ 或 $tag$...$tag$
            // 对应上游 commit 95ebda5a
            var text = context.S_DOLLAR_QUOTED_STRING().GetText();
            return new StringValue(text);
        }
        if (context.S_HEX() != null)
            return new HexValue { Value = context.S_HEX().GetText() };
        if (context.NULL() != null)
            return new NullValue();
        if (context.TRUE() != null)
            return new BooleanValue(true);
        if (context.FALSE() != null)
            return new BooleanValue(false);
        if (context.dateTimeLiteral() != null)
            return Visit(context.dateTimeLiteral());

        return new NullValue();
    }

    public override object VisitDateTimeLiteral(JSqlParserGrammar.DateTimeLiteralContext context)
    {
        // 取类型 token：DATE / DATETIME / TIME / TIMESTAMP / TIMESTAMPTZ
        var typeText = context.DATE()?.GetText()
            ?? context.DATETIME()?.GetText()
            ?? context.TIME()?.GetText()
            ?? context.TIMESTAMP()?.GetText()
            ?? context.TIMESTAMPTZ()?.GetText() ?? "";
        // 取值：保留原始 token 文本（含引号），对齐上游 expr.setValue(t.image) 的存储行为
        var value = (context.S_CHAR_LITERAL() ?? context.QUOTED_IDENTIFIER()).GetText();

        return new DateTimeLiteralExpression
        {
            // SQL 关键字（TIMESTAMP 等）解析为 DateTimeType 枚举，忽略大小写匹配 PascalCase 枚举名
            Type = Enum.Parse<DateTimeType>(typeText, ignoreCase: true),
            Value = value
        };
    }

    public override object VisitParameter(JSqlParserGrammar.ParameterContext context)
    {
        if (context.S_JDBC_NAMED_PARAM() != null)
        {
            return new JdbcNamedParameter { Name = context.S_JDBC_NAMED_PARAM().GetText()[1..] };
        }

        if (context.SINGLE_AT_IDENTIFIER() != null)
        {
            return new JdbcNamedParameter { Name = context.SINGLE_AT_IDENTIFIER().GetText()[1..], Prefix = "@" };
        }

        // :1、:2 数值绑定（Oracle/MySQL），与命名参数共用 JdbcNamedParameter，Name 存数字串
        // grammar 用 COLON LONG_VALUE 组合避免与数组范围 [1:3] 的冒号冲突
        if (context.LONG_VALUE() != null && context.COLON() != null)
        {
            return new JdbcNamedParameter { Name = context.LONG_VALUE().GetText() };
        }

        var param = new JdbcParameter();
        if (context.S_PARAMETER() != null)
        {
            param.Index = ParseInt(context.S_PARAMETER().GetText()[1..]);
        }
        return param;
    }

    public override object VisitCaseExpr(JSqlParserGrammar.CaseExprContext context)
    {
        var caseExpr = new CaseExpression();

        // 仅当 CASE 后直接跟 switch 表达式时（switch 形式）才赋值 SwitchExpression。
        // 不能用 context.expression().Length 或 GetChild<ExpressionContext>(0) 判断：
        //   - context.expression() 会递归收集 whenExpr 内嵌的 cond/then 及 ELSE 表达式
        //   - GetChild<ExpressionContext>(0) 返回首个 ExpressionContext 类型的直接子节点，
        //     在 searched 形式下那其实是 ELSE 表达式，会被错误地当作 switch 表达式，
        //     round-trip 输出形如 "CASE 'small' WHEN a > 1 THEN 'big' ... END"（语义错误）。
        // 正确判断：CASE 关键字（child[0]）之后的 child[1] 是否为 ExpressionContext。
        //   - switch 形式：child[1] = ExpressionContext（switch 操作数）
        //   - searched 形式：child[1] = WhenExprContext（首个 WHEN）
        var switchExprChild = context.GetChild(1) as JSqlParserGrammar.ExpressionContext;
        if (switchExprChild != null)
        {
            caseExpr.SwitchExpression = (Expression.IExpression)Visit(switchExprChild);
        }

        caseExpr.WhenClauses = new List<WhenClause>();

        foreach (var whenCtx in context.whenExpr())
        {
            var whenClause = new WhenClause
            {
                WhenExpression = (Expression.IExpression)Visit(whenCtx.expression(0)),
                ThenExpression = (Expression.IExpression)Visit(whenCtx.expression(1))
            };
            caseExpr.WhenClauses.Add(whenClause);
        }

        if (context.ELSE() != null)
        {
            var elseExprs = context.expression();
            caseExpr.ElseExpression = (Expression.IExpression)Visit(elseExprs[^1]);
        }

        return caseExpr;
    }

    public override object VisitCastExpr(JSqlParserGrammar.CastExprContext context)
    {
        return new CastExpression
        {
            Expression = (Expression.IExpression)Visit(context.expression()),
            DataType = context.dataType().GetText(),
            // 设置 CAST 关键字：CAST / TRY_CAST / SAFE_CAST
            Keyword = context.SAFE_CAST() != null ? "SAFE_CAST"
                : context.TRY_CAST() != null ? "TRY_CAST" : "CAST"
        };
    }

    public override object VisitExtractExpr(JSqlParserGrammar.ExtractExprContext context)
    {
        return new ExtractExpression
        {
            Name = context.extractField().GetText(),
            Expression = (Expression.IExpression)Visit(context.expression())
        };
    }

    public override object VisitIntervalExpr(JSqlParserGrammar.IntervalExprContext context)
    {
        var interval = new IntervalExpression();
        interval.IntervalKeyword = true;
        interval.Expression = (Expression.IExpression)Visit(context.expression());

        if (context.YEAR() != null) interval.IntervalType = "YEAR";
        else if (context.MONTH() != null) interval.IntervalType = "MONTH";
        else if (context.DAY() != null) interval.IntervalType = "DAY";
        else if (context.HOUR() != null) interval.IntervalType = "HOUR";
        else if (context.MINUTE() != null) interval.IntervalType = "MINUTE";
        else if (context.SECOND() != null) interval.IntervalType = "SECOND";

        return interval;
    }

    public override object VisitExpressionList(JSqlParserGrammar.ExpressionListContext context)
    {
        var list = new ExpressionList();
        list.Expressions = new List<Expression.IExpression>();
        foreach (var expr in context.expression())
        {
            list.Expressions.Add((Expression.IExpression)Visit(expr));
        }
        return list;
    }

    public override object VisitColumnRef(JSqlParserGrammar.ColumnRefContext context)
    {
        var identifiers = context.identifier();
        Column column;
        if (identifiers.Length == 1)
        {
            column = new Column { ColumnName = identifiers[0].GetText() };
        }
        else if (identifiers.Length == 2)
        {
            var table = new Table { Name = identifiers[0].GetText() };
            column = new Column { Table = table, ColumnName = identifiers[1].GetText() };
        }
        else
        {
            column = new Column { ColumnName = context.GetText() };
        }

        // Oracle 老式外连接语法 column(+) — commit 834afe18
        if (context.oracleOuterJoinSuffix() != null)
        {
            column.OldOracleJoinSyntax = OracleJoinSyntax.Right;
        }
        return column;
    }

    public override object VisitTable(JSqlParserGrammar.TableContext context)
    {
        var identifiers = context.identifier();
        // 支持 1-4 段命名：name / schema.name / db.schema.name / server.db.schema.name
        return identifiers.Length switch
        {
            1 => new Table { Name = identifiers[0].GetText() },
            2 => new Table { SchemaName = identifiers[0].GetText(), Name = identifiers[1].GetText() },
            3 => new Table
            {
                Database = identifiers[0].GetText(),
                SchemaName = identifiers[1].GetText(),
                Name = identifiers[2].GetText()
            },
            _ => new Table
            {
                ServerName = identifiers[0].GetText(),
                Database = identifiers[1].GetText(),
                SchemaName = identifiers[2].GetText(),
                Name = identifiers[3].GetText()
            }
        };
    }

    public override object VisitLambdaExpression(JSqlParserGrammar.LambdaExpressionContext context)
    {
        var identifiers = new List<string>();
        if (context.identifierList() != null)
        {
            foreach (var id in context.identifierList().identifier())
                identifiers.Add(id.GetText());
        }
        else if (context.identifier() != null)
        {
            identifiers.Add(context.identifier().GetText());
        }

        return new LambdaExpression(identifiers, (Expression.IExpression)Visit(context.expression()));
    }

    public override object VisitStructType(JSqlParserGrammar.StructTypeContext context)
    {
        var structType = new StructType();

        // DuckDB syntax: { a::expr, b::expr } [::STRUCT(...)]
        if (context.LBRACE() != null)
        {
            structType.StructDialect = StructType.Dialect.DuckDB;
            structType.Arguments = new List<SelectItem>();
            foreach (var argCtx in context.structArgument())
            {
                var id = argCtx.identifier()?.GetText() ?? argCtx.S_CHAR_LITERAL()?.GetText().Trim('\'');
                var expr = (Expression.IExpression)Visit(argCtx.expression());
                var item = new SelectItem { Expression = expr };
                if (id != null) item.Alias = new Alias { Name = id };
                structType.Arguments.Add(item);
            }

            var structParams = context.structParameters();
            if (structParams != null)
            {
                structType.Parameters = new List<KeyValuePair<string, string>>();
                foreach (var paramCtx in structParams.structParameter())
                {
                    var paramName = paramCtx.identifier()?.GetText() ?? "";
                    var paramType = paramCtx.dataType().GetText();
                    structType.Parameters.Add(new KeyValuePair<string, string>(paramName, paramType));
                }
            }

            return structType;
        }

        // BigQuery syntax: STRUCT<params>(args) or STRUCT(args)
        structType.StructDialect = StructType.Dialect.BigQuery;
        structType.Keyword = context.STRUCT().GetText();

        var parameters = context.structParameters();
        if (parameters != null)
        {
            structType.Parameters = new List<KeyValuePair<string, string>>();
            foreach (var paramCtx in parameters.structParameter())
            {
                var paramName = paramCtx.identifier()?.GetText() ?? "";
                var paramType = paramCtx.dataType().GetText();
                structType.Parameters.Add(new KeyValuePair<string, string>(paramName, paramType));
            }
        }

        if (context.selectColumnList() != null)
        {
            structType.Arguments = new List<SelectItem>();
            foreach (var itemCtx in context.selectColumnList().selectItem())
            {
                var item = (SelectItem)Visit(itemCtx);
                structType.Arguments.Add(item);
            }
        }

        return structType;
    }

    public override object VisitPreferringClause(JSqlParserGrammar.PreferringClauseContext context)
    {
        var preferring = new PreferringClause((Expression.IExpression)Visit(context.preferenceTerm()));

        if (context.expressionList() != null)
        {
            preferring.PartitionBy = (ExpressionList)Visit(context.expressionList());
        }

        return preferring;
    }

    public override object VisitPreferenceTerm(JSqlParserGrammar.PreferenceTermContext context)
    {
        if (context.HIGH() != null)
        {
            return new HighExpression((Expression.IExpression)Visit(context.expression()));
        }
        if (context.LOW() != null)
        {
            return new LowExpression((Expression.IExpression)Visit(context.expression()));
        }
        if (context.INVERSE_KW() != null)
        {
            return new Inverse((Expression.IExpression)Visit(context.expression()));
        }
        if (context.PLUS_KW() != null)
        {
            var inner = (Expression.IExpression)Visit(context.preferenceTerm());
            return inner; // Plus is a binary operator, but here it's unary prefix
        }
        if (context.PRIOR() != null)
        {
            var inner = (Expression.IExpression)Visit(context.preferenceTerm());
            return inner; // PriorTo is binary, but here it's unary prefix
        }
        if (context.expression() != null)
        {
            return (Expression.IExpression)Visit(context.expression());
        }

        return new NullValue();
    }

    public override object VisitConnectByPriorOperator(JSqlParserGrammar.ConnectByPriorOperatorContext context)
    {
        return new ConnectByPriorOperator((Expression.IExpression)Visit(context.expression()));
    }

    public override object VisitConnectByRootOperator(JSqlParserGrammar.ConnectByRootOperatorContext context)
    {
        return new ConnectByRootOperator((Expression.IExpression)Visit(context.expression()));
    }
}
