using System;
using System.Linq.Expressions;
#if NET7_0_OR_GREATER
using Microsoft.EntityFrameworkCore.Query;
#endif

namespace Azrng.EFCore.Extensions
{
    /// <summary>
    /// 表达式树扩展
    /// </summary>
    public static class ExpressExtensions
    {
#if NET7_0_OR_GREATER && (!NET10_0_OR_GREATER)
        /// <summary>
        /// 根据条件追加
        /// </summary>
        /// <param name="left">左侧逻辑</param>
        /// <param name="condition">条件</param>
        /// <param name="right">右侧逻辑</param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> AppendIf<TEntity>(
            this Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> left, bool condition,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> right)
        {
            if (!condition)
                return left;

            var replace = new ReplacingExpressionVisitor(right.Parameters, new[]
                                                                           {
                                                                               left.Body
                                                                           });
            var combined = replace.Visit(right.Body);
            return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(combined,
                left.Parameters);
        }
#endif
    }
}