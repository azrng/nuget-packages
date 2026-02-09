using Azrng.Core.Extension;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq.Expressions;

namespace Azrng.EFCore.Extensions
{
#if NET10_0_OR_GREATER
    /// <summary>
    /// 条件更新扩展方法
    /// </summary>
    public static class ConditionalUpdateExtensions
    {
        /// <summary>
        /// 当条件为 true 时，指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值
        /// </summary>
        /// <typeparam name="TSource">实体类型</typeparam>
        /// <typeparam name="TProperty">属性的类型</typeparam>
        /// <param name="updateSettersBuilder">更新设置构建器</param>
        /// <param name="condition">条件，为 true 时才更新属性</param>
        /// <param name="propertyExpression">属性访问表达式</param>
        /// <param name="valueExpression">要更新的值</param>
        /// <returns>更新设置构建器，支持链式调用</returns>
        public static UpdateSettersBuilder<TSource> SetPropertyIfTrue<TSource, TProperty>(
            this UpdateSettersBuilder<TSource> updateSettersBuilder,
            bool condition,
            Expression<Func<TSource, TProperty>> propertyExpression,
            TProperty valueExpression)
        {
            if (!condition)
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, Expression.Constant(valueExpression, typeof(TProperty)));
            return updateSettersBuilder;
        }

        /// <summary>
        /// 当值不为 null 或空白时，指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值
        /// </summary>
        /// <typeparam name="TSource">实体类型</typeparam>
        /// <param name="updateSettersBuilder">更新设置构建器</param>
        /// <param name="propertyExpression">属性访问表达式</param>
        /// <param name="valueExpression">要更新的值（当为 null 或空白时不更新）</param>
        /// <returns>更新设置构建器，支持链式调用</returns>
        public static UpdateSettersBuilder<TSource> SetPropertyIfNotNullOrWhiteSpace<TSource>(
            this UpdateSettersBuilder<TSource> updateSettersBuilder,
            Expression<Func<TSource, string>> propertyExpression,
            string valueExpression)
        {
            if (valueExpression.IsNullOrWhiteSpace())
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, Expression.Constant(valueExpression, typeof(string)));
            return updateSettersBuilder;
        }

        /// <summary>
        /// 当值不为 null 时，指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值
        /// </summary>
        /// <typeparam name="TSource">实体类型</typeparam>
        /// <typeparam name="TProperty">属性的类型（引用类型）</typeparam>
        /// <param name="updateSettersBuilder">更新设置构建器</param>
        /// <param name="propertyExpression">属性访问表达式</param>
        /// <param name="valueExpression">要更新的值（当为 null 时不更新）</param>
        /// <returns>更新设置构建器，支持链式调用</returns>
        public static UpdateSettersBuilder<TSource> SetPropertyIfNotNull<TSource, TProperty>(
            this UpdateSettersBuilder<TSource> updateSettersBuilder,
            Expression<Func<TSource, TProperty>> propertyExpression,
            TProperty valueExpression)
            where TProperty : class
        {
            if (valueExpression is null)
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, Expression.Constant(valueExpression, typeof(TProperty)));
            return updateSettersBuilder;
        }

        /// <summary>
        /// 当满足自定义条件时，指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值
        /// </summary>
        /// <typeparam name="TSource">实体类型</typeparam>
        /// <typeparam name="TProperty">属性的类型</typeparam>
        /// <param name="updateSettersBuilder">更新设置构建器</param>
        /// <param name="condition">条件函数，传入要更新的值，返回是否应该更新</param>
        /// <param name="propertyExpression">属性访问表达式</param>
        /// <param name="valueExpression">要更新的值</param>
        /// <returns>更新设置构建器，支持链式调用</returns>
        public static UpdateSettersBuilder<TSource> SetPropertyIf<TSource, TProperty>(
            this UpdateSettersBuilder<TSource> updateSettersBuilder,
            Func<TProperty, bool> condition,
            Expression<Func<TSource, TProperty>> propertyExpression,
            TProperty valueExpression)
        {
            if (!condition(valueExpression))
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, Expression.Constant(valueExpression, typeof(TProperty)));
            return updateSettersBuilder;
        }
    }
#endif
}
