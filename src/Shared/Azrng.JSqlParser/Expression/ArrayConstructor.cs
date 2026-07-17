using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 数组构造器：<c>ARRAY[1, 2, 3]</c> 或 <c>[1, 2, 3]</c>（PostgreSQL/HSQLDB/ClickHouse）。
/// 与上游 ArrayConstructor 对齐。
/// </summary>
public class ArrayConstructor : ASTNodeAccessImpl, Expression
{
    /// <summary>数组元素表达式列表。</summary>
    public ExpressionList? Expressions { get; set; }

    /// <summary>true 表示使用 ARRAY 关键字前缀；false 表示仅 [...]</summary>
    public bool ArrayKeyword { get; set; }

    public ArrayConstructor() { }

    public ArrayConstructor(ExpressionList? expressions, bool arrayKeyword)
    {
        Expressions = expressions;
        ArrayKeyword = arrayKeyword;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        if (ArrayKeyword) sb.Append("ARRAY");
        sb.Append('[').Append(Expressions).Append(']');
        return sb.ToString();
    }
}

/// <summary>
/// 数组下标访问表达式：<c>arr[index]</c> 或范围 <c>arr[start:end]</c>。
/// 与上游 ArrayExpression 对齐。
/// </summary>
public class ArrayExpression : ASTNodeAccessImpl, Expression
{
    /// <summary>被索引的对象表达式（数组/JSON）。</summary>
    public Expression? ObjExpression { get; set; }

    /// <summary>单下标索引表达式（与范围互斥）。</summary>
    public Expression? IndexExpression { get; set; }

    /// <summary>范围起始索引（与 IndexExpression 互斥）。</summary>
    public Expression? StartIndexExpression { get; set; }

    /// <summary>范围结束索引（与 IndexExpression 互斥）。</summary>
    public Expression? StopIndexExpression { get; set; }

    public ArrayExpression() { }

    public ArrayExpression(Expression? objExpression, Expression? indexExpression)
    {
        ObjExpression = objExpression;
        IndexExpression = indexExpression;
    }

    public ArrayExpression(Expression? objExpression, Expression? startIndexExpression, Expression? stopIndexExpression)
    {
        ObjExpression = objExpression;
        StartIndexExpression = startIndexExpression;
        StopIndexExpression = stopIndexExpression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (IndexExpression != null)
        {
            return $"{ObjExpression}[{IndexExpression}]";
        }
        return $"{ObjExpression}[{StartIndexExpression}:{StopIndexExpression}]";
    }
}
