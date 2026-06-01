using JSqlParser.Net.Parser;
using JSqlParser.Net.Statement.Delete;
using JSqlParser.Net.Statement.Insert;
using JSqlParser.Net.Statement.Select;
using JSqlParser.Net.Statement.Update;

namespace JSqlParser.Net.Util.Validation;

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
            _parsedStatements = CCJSqlParserUtil.ParseStatements(_sql);
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
    /// Get the parsed statements (available after validate() is called).
    /// </summary>
    public Statement.Statements? GetParsedStatements() => _parsedStatements;

    /// <summary>
    /// Get the validation errors (available after validate() is called).
    /// </summary>
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
        }
    }

    private void ValidateSubquery(Expression.Expression? expression)
    {
        if (expression == null) return;
        // Basic subquery detection - if expression contains a ParenthesedSelect
        var text = expression.ToString();
        if (text == null) return;

        if (text.Contains("SELECT", StringComparison.OrdinalIgnoreCase))
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
