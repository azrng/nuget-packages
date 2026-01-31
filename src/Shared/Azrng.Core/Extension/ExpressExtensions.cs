using Azrng.Core.CommonDto;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Azrng.Core.Extension
{
    public static class ExpressExtensions
    {
        #region 连接

        /// <summary>
        /// 添加And条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.AndAlso(second, Expression.AndAlso);
        }

        /// <summary>
        /// 添加Or条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.AndAlso(second, Expression.OrElse);
        }

        /// <summary>
        /// 合并表达式以及参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2,
                                                            Func<Expression, Expression, BinaryExpression> func)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(func(left, right), parameter);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }

        #endregion

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
            Expression<Func<T, bool>>? result = express;

            //合并表达式
            foreach (var curExpression in arrayExpress)
            {
                //表达式树内容
                var left = result != null ? visitor.Visit(result.Body) : null;
                var right = visitor.Visit(curExpression.Body);
                result = Expression.Lambda<Func<T, bool>>(
                    left != null ? Expression.AndAlso(left, right) : right,
                    parameter);
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
            Expression<Func<T, bool>>? result = express;

            //合并表达式
            foreach (var curExpression in arrayExpress)
            {
                //表达式树内容
                var left = result != null ? visitor.Visit(result.Body) : null;
                var right = visitor.Visit(curExpression.Body);
                result = Expression.Lambda<Func<T, bool>>(
                    left != null ? Expression.OrElse(left, right) : right,
                    parameter);
            }

            return result;
        }

        #endregion
    }
}