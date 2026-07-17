using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.CreateIndex;
using Azrng.JSqlParser.Statement.CreateTable;
using Azrng.JSqlParser.Statement.CreateView;
using Azrng.JSqlParser.Statement.Delete;
using Azrng.JSqlParser.Statement.Drop;
using Azrng.JSqlParser.Statement.Insert;
using Azrng.JSqlParser.Statement.Merge;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Truncate;
using Azrng.JSqlParser.Statement.Update;

namespace Azrng.JSqlParser.Util.Validation;

/// <summary>
/// SQL validation framework. Validates SQL against allowed feature capabilities.
/// </summary>
public class Validation
{
    private readonly List<FeaturesAllowed> _capabilities;
    private readonly string _sql;
    private List<ValidationError> _errors = new();
    private Statement.Statements? _parsedStatements;

    public Validation(List<FeaturesAllowed> capabilities, string sql)
    {
        _capabilities = capabilities;
        _sql = sql;
    }

    /// <summary>
    /// Validate the SQL against the allowed capabilities.
    /// </summary>
    public List<ValidationError> Validate()
    {
        _errors = new List<ValidationError>();

        try
        {
            _parsedStatements = SqlParser.ParseStatements(_sql);
            if (_parsedStatements != null)
            {
                ValidateStatements(_parsedStatements);
            }
        }
        catch (JSqlParserException ex)
        {
            _errors.Add(new ValidationError(ex.Message));
        }

        return _errors;
    }

    /// <summary>
    /// 解析后的语句（调用 <see cref="Validate"/> 后可用）。
    /// </summary>
    public Statement.Statements? ParsedStatements => _parsedStatements;

    /// <summary>
    /// 校验错误列表（调用 <see cref="Validate"/> 后可用）。
    /// </summary>
    public IReadOnlyList<ValidationError> Errors => _errors;

    /// <summary>解析后的语句（兼容旧 API，改用 <see cref="ParsedStatements"/> 属性）。</summary>
    [Obsolete("改用 " + nameof(ParsedStatements) + " 属性")]
    public Statement.Statements? GetParsedStatements() => _parsedStatements;

    /// <summary>校验错误列表（兼容旧 API，改用 <see cref="Errors"/> 属性）。</summary>
    [Obsolete("改用 " + nameof(Errors) + " 属性")]
    public List<ValidationError> GetErrors() => _errors;

    private void ValidateStatements(Statement.Statements statements)
    {
        foreach (var stmt in statements.StatementList)
        {
            ValidateStatement(stmt);
        }
    }

    private void ValidateStatement(Statement.Statement statement)
    {
        switch (statement)
        {
            case Select:
                RequireFeature(FeaturesAllowed.SELECT);
                if (statement is PlainSelect plainSelect)
                {
                    if (plainSelect.Joins != null && plainSelect.Joins.Count > 0)
                        RequireFeature(FeaturesAllowed.JOIN);
                    ValidateSubquery(plainSelect.Where);
                }
                else if (statement is SetOperationList setOpList)
                {
                    foreach (var op in setOpList.Operations)
                    {
                        switch (op.Type)
                        {
                            case SetOperation.OperationType.UNION:
                                RequireFeature(FeaturesAllowed.UNION);
                                break;
                            case SetOperation.OperationType.EXCEPT:
                            case SetOperation.OperationType.MINUS:
                                // Oracle MINUS 等价 EXCEPT，归入同一能力
                                RequireFeature(FeaturesAllowed.EXCEPT);
                                break;
                            case SetOperation.OperationType.INTERSECT:
                                RequireFeature(FeaturesAllowed.INTERSECT);
                                break;
                        }
                    }
                }
                break;
            case Insert:
                RequireFeature(FeaturesAllowed.INSERT);
                break;
            case Update:
                RequireFeature(FeaturesAllowed.UPDATE);
                break;
            case Delete:
                RequireFeature(FeaturesAllowed.DELETE);
                break;
            case Merge:
                RequireFeature(FeaturesAllowed.MERGE);
                break;
            case CreateTable:
                RequireFeature(FeaturesAllowed.CREATE);
                break;
            case CreateView:
                RequireFeature(FeaturesAllowed.CREATE);
                break;
            case CreateIndex:
                RequireFeature(FeaturesAllowed.CREATE);
                break;
            case Alter:
            case AlterView:
            case AlterSequence:
                RequireFeature(FeaturesAllowed.ALTER);
                break;
            case Drop:
                RequireFeature(FeaturesAllowed.DROP);
                break;
            case Truncate:
                RequireFeature(FeaturesAllowed.TRUNCATE);
                break;
        }
    }

    private void ValidateSubquery(Expression.Expression? expression)
    {
        if (expression == null) return;
        // 子查询检测：通过 ToString 文本启发式判断是否含 SELECT 子查询。
        // 局限性：字面量或列名含 "SELECT" 子串可能误报；精确检测需 visitor 遍历表达式树。
        // 为降低误报，要求 SELECT 前有左括号（子查询形如 (SELECT ...)）或作为 IN/EXISTS 操作数。
        var text = expression.ToString();
        if (text == null) return;

        if (text.Contains("(SELECT", StringComparison.OrdinalIgnoreCase)
            || text.Contains("( SELECT", StringComparison.OrdinalIgnoreCase))
        {
            RequireFeature(FeaturesAllowed.SUBQUERY);
        }
    }

    private void RequireFeature(FeaturesAllowed feature)
    {
        if (_capabilities.Count > 0 && !_capabilities.Contains(feature))
        {
            _errors.Add(new ValidationError($"Feature not allowed: {feature}", feature));
        }
    }
}
