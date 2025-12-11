using Azrng.Core.CommonDto;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Azrng.Core.Extensions
{
    public static class ExpressExtensions
    {
        #region 查询

        /// <summary>
        /// 值对象比较
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="propAccessor">待比较的表达式</param>
        /// <param name="other">待比较的值对象</param>
        /// <returns>
        ///  _openDbContext.ContainsValueObjectUsers.Where(ExpressionHelper.MarkEqual((ContainsValueObjectUser area) => area.Area, new ValueObjectArea("河南", "焦作"))).FirstOrDefaultAsync();
        /// </returns>
        public static Expression<Func<TItem, bool>> MarkEqual<TItem, TProp>(
            this Expression<Func<TItem, TProp>> propAccessor,
            TProp other)
            where TItem : class
            where TProp : class
        {
            var e1 = propAccessor.Parameters.Single();
            BinaryExpression? conditionalExpr = null;
            foreach (var prop in typeof(TProp).GetProperties())
            {
                BinaryExpression equalExpr;
                object? otherValue = null;
                if (other != null)
                    otherValue = prop.GetValue(other);

                var propType = prop.PropertyType;
                var leftExpr = Expression.MakeMemberAccess(propAccessor.Body, prop);
                Expression rightExpr = Expression.Constant(otherValue, propType);
                if (propType.IsPrimitive)
                {
                    equalExpr = Expression.Equal(leftExpr, rightExpr);
                }
                else
                {
                    equalExpr = Expression.MakeBinary(ExpressionType.Equal, leftExpr, rightExpr, false,
                        prop.PropertyType.GetMethod("op_Equality"));
                }

                conditionalExpr = conditionalExpr is null ? equalExpr : Expression.AndAlso(conditionalExpr, equalExpr);
            }

            return Expression.Lambda<Func<TItem, bool>>(conditionalExpr, e1);
        }

        #endregion

        #region 筛选

        /// <summary>
        /// 以And合并单个表达式
        /// 此处采用AndAlso实现“最短路径”，避免掉额外且不需要的比较运算式
        /// </summary>
        public static Expression<Func<T, bool>> MergeAnd<T>(this Expression<Func<T, bool>> leftExpress,
                                                            Expression<Func<T, bool>> rightExpress)
        {
            //声明传递参数（也就是表达式树里面的参数别名s）
            var parameter = Expression.Parameter(typeof(T), "s");

            //统一管理参数，保证参数一致，否则会报错
            var visitor = new PredicateExpressionVisitor(parameter);

            //表达式树内容
            var left = visitor.Visit(leftExpress.Body);
            var right = visitor.Visit(rightExpress.Body);

            //合并表达式
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }

        /// <summary>
        /// 以And合并多个表达式
        /// 此处采用AndAlso实现“最短路径”，避免掉额外且不需要的比较运算式
        /// </summary>
        public static Expression<Func<T, bool>>? MergeAnd<T>(this Expression<Func<T, bool>>? express,
                                                             params Expression<Func<T, bool>>[] arrayExpress)
        {
            if (!arrayExpress?.Any() ?? true) return express;

            //声明传递参数（也就是表达式树里面的参数别名s）
            var parameter = Expression.Parameter(typeof(T), "s");

            //统一管理参数，保证参数一致，否则会报错
            var visitor = new PredicateExpressionVisitor(parameter);
            Expression<Func<T, bool>>? result = null;

            //合并表达式
            foreach (var curExpression in arrayExpress)
            {
                //表达式树内容
                var left = visitor.Visit(result.Body);
                var right = visitor.Visit(curExpression.Body);
                result = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
            }

            return result;
        }

        /// <summary>
        /// 以Or合并表达式
        /// 此处采用OrElse实现“最短路径”，避免掉额外且不需要的比较运算式
        /// </summary>
        public static Expression<Func<T, bool>> MergeOr<T>(this Expression<Func<T, bool>> leftExpress,
                                                           Expression<Func<T, bool>> rightExpress)
        {
            //声明传递参数（也就是表达式树里面的参数别名s）
            var parameter = Expression.Parameter(typeof(T), "s");

            //统一管理参数，保证参数一致，否则会报错
            var visitor = new PredicateExpressionVisitor(parameter);

            //表达式树内容
            var left = visitor.Visit(leftExpress.Body);
            var right = visitor.Visit(rightExpress.Body);

            //合并表达式
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left, right), parameter);
        }

        /// <summary>
        /// 以Or合并多个表达式
        /// 此处采用AndAlso实现“最短路径”，避免掉额外且不需要的比较运算式
        /// </summary>
        public static Expression<Func<T, bool>>? MergeOr<T>(this Expression<Func<T, bool>>? express,
                                                            params Expression<Func<T, bool>>[] arrayExpress)
        {
            if (!arrayExpress?.Any() ?? true) return express;

            //声明传递参数（也就是表达式树里面的参数别名s）
            var parameter = Expression.Parameter(typeof(T), "s");

            //统一管理参数，保证参数一致，否则会报错
            var visitor = new PredicateExpressionVisitor(parameter);
            Expression<Func<T, bool>>? result = null;

            //合并表达式
            foreach (var curExpression in arrayExpress)
            {
                //表达式树内容
                var left = visitor.Visit(result.Body);
                var right = visitor.Visit(curExpression.Body);
                result = Expression.Lambda<Func<T, bool>>(Expression.OrElse(left, right), parameter);
            }

            return result;
        }

        #endregion
    }
}