using System;
using System.Linq.Expressions;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// ParameterExpression
    /// </summary>
    internal class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _source;

        public ParameterReplacer(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            ParameterExpression = target;
        }

        public ParameterExpression ParameterExpression { get; }

        public Expression Replace(Expression expr)
        {
            return Visit(expr);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            //仅替换来源参数；嵌套 lambda（如 Any/Where 内层）声明的参数保持原样
            return p == _source ? ParameterExpression : p;
        }
    }

    /// <summary>
    /// PredicateExtensions
    /// </summary>
    public static class PredicateExtensions
    {
        /// <summary>
        /// Linq函数应用True时：单个AND有效，多个AND有效；单个OR无效，多个OR无效；混应时写在AND后的OR有效
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() => _ => true;

        /// <summary>
        /// Linq函数应用False时：单个AND无效，多个AND无效；单个OR有效，多个OR有效；混应时写在OR后面的AND有效
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() => _ => false;

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> exp_left,
            Expression<Func<T, bool>> exp_right)
        {
            var candidateExpr = Expression.Parameter(typeof(T), "candidate");
            //各来源 lambda 的顶层参数单独映射，嵌套 lambda 内层参数保持原样
            var left = new ParameterReplacer(exp_left.Parameters[0], candidateExpr).Replace(exp_left.Body);
            var right = new ParameterReplacer(exp_right.Parameters[0], candidateExpr).Replace(exp_right.Body);
            var body = Expression.And(left, right);

            return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> exp_left,
            Expression<Func<T, bool>> exp_right)
        {
            var candidateExpr = Expression.Parameter(typeof(T), "candidate");
            var left = new ParameterReplacer(exp_left.Parameters[0], candidateExpr).Replace(exp_left.Body);
            var right = new ParameterReplacer(exp_right.Parameters[0], candidateExpr).Replace(exp_right.Body);
            var body = Expression.Or(left, right);

            return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
        }
        
        public static Expression GreaterThan(this Expression left, Expression right)
        {
            return Expression.GreaterThan(left, right);
        }
    }
}