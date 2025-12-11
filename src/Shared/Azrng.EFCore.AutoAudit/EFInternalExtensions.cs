using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Azrng.EFCore.AutoAudit;

internal static class EfInternalExtensions
{
    /// <summary>
    /// 获取实体属性在数据库中的列名。
    /// </summary>
    /// <param name="propertyEntry">实体属性的跟踪信息</param>
    /// <returns>数据库中的列名，若不存在则返回属性名称。</returns>
    public static string GetColumnName(this PropertyEntry propertyEntry)
    {
        // 获取当前属性的表标识（如：表名）
        var storeObjectId =
            StoreObjectIdentifier.Create(propertyEntry.Metadata.DeclaringType, StoreObjectType.Table);

        // 从属性元数据中获取列名，如果不存在则返回属性名称
        return propertyEntry.Metadata.GetColumnName(storeObjectId.GetValueOrDefault())
               ?? propertyEntry.Metadata.Name;
    }

    /// <summary>
    /// 从 Lambda 表达式中提取成员名称。
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TMember">成员类型（如：int、string 等）</typeparam>
    /// <param name="memberExpression">Lambda 表达式，如：e => e.Name</param>
    /// <returns>成员名称（例如："Name"）</returns>
    public static string GetMemberName<TEntity, TMember>(this Expression<Func<TEntity, TMember>> memberExpression)
    {
        return memberExpression.GetMemberInfo().Name;
    }

    /// <summary>
    /// 从 Lambda 表达式中提取 MemberInfo 对象（如 PropertyInfo、MethodInfo）。
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TMember">成员类型（如：int、string 等）</typeparam>
    /// <param name="expression">Lambda 表达式，如：e => e.Name</param>
    /// <returns>成员的 MemberInfo 对象。</returns>
    public static MemberInfo GetMemberInfo<TEntity, TMember>(this Expression<Func<TEntity, TMember>> expression)
    {
        // 确保表达式是 Lambda 表达式
        if (expression.NodeType != ExpressionType.Lambda)
        {
            throw new ArgumentException(
                string.Format(Resource.propertyExpression_must_be_lambda_expression, "expression"), "expression");
        }

        // 提取表达式体中的 MemberExpression
        var memberExpr = ExtractMemberExpression(expression.Body)
                         ?? throw new ArgumentException(
                             string.Format(Resource.propertyExpression_must_be_lambda_expression, "expression"), "expression");

        return memberExpr.Member;
    }

    /// <summary>
    /// 从表达式中提取 MemberExpression，处理可能的转换（Convert）节点。
    /// </summary>
    /// <param name="expression">要提取的表达式</param>
    /// <returns>MemberExpression 对象，如果无法提取则抛出异常。</returns>
    private static MemberExpression ExtractMemberExpression(Expression expression)
    {
        // 如果是成员访问表达式，直接返回
        if (expression.NodeType == ExpressionType.MemberAccess)
        {
            return (MemberExpression)expression;
        }

        // 如果是转换表达式（如：Convert），继续提取其操作数
        if (expression.NodeType == ExpressionType.Convert)
        {
            return ExtractMemberExpression(((UnaryExpression)expression).Operand);
        }

        // 其他表达式类型不支持
        throw new InvalidOperationException("ExtractMemberExpression");
    }
}
