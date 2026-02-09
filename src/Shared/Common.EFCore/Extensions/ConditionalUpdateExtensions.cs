using Azrng.Core.Extension;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq.Expressions;

namespace Azrng.EFCore.Extensions
{
    /// <summary>
    /// 条件更新扩展方法
    /// </summary>
    public static class ConditionalUpdateExtensions
    {
#if NET7_0_OR_GREATER && (!NET10_0_OR_GREATER)
        /// <summary>
        /// 指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值。
        /// </summary>
        /// <typeparam name="TProperty">属性的类型。</typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="condition">条件</param>
        /// <param name="propertyExpression">一个属性访问表达式。</param>
        /// <param name="valueExpression">一个值表达式。</param>
        /// <param name="updateSettersBuilder"></param>
        public static SetPropertyCalls<TSource> SetPropertyIfTrue<TSource, TProperty>(
            this SetPropertyCalls<TSource> updateSettersBuilder,
            bool condition,
            Func<TSource, TProperty> propertyExpression,
            TProperty valueExpression)
        {
            if (!condition)
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, valueExpression);
            return updateSettersBuilder;
        }

        /// <summary>
        /// 指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="propertyExpression">一个属性访问表达式。</param>
        /// <param name="valueExpression">一个值表达式。</param>
        /// <param name="updateSettersBuilder"></param>
        public static SetPropertyCalls<TSource> SetPropertyIfNotNullOrWhiteSpace<TSource>(
            this SetPropertyCalls<TSource> updateSettersBuilder,
            Func<TSource, string> propertyExpression,
            string valueExpression)
        {
            if (valueExpression.IsNullOrWhiteSpace())
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, valueExpression);
            return updateSettersBuilder;
        }

        /// <summary>
        /// 指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值。
        /// </summary>
        /// <typeparam name="TProperty">属性的类型。</typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="propertyExpression">一个属性访问表达式。</param>
        /// <param name="valueExpression">一个值表达式。</param>
        /// <param name="updateSettersBuilder"></param>
        public static SetPropertyCalls<TSource> SetPropertyIfNotNull<TSource, TProperty>(
            this SetPropertyCalls<TSource> updateSettersBuilder,
            Func<TSource, TProperty> propertyExpression,
            TProperty valueExpression)
        {
            if (valueExpression is null)
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, valueExpression);
            return updateSettersBuilder;
        }

        /// <summary>
        /// 指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值。
        /// </summary>
        /// <typeparam name="TProperty">属性的类型。</typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="condition">条件</param>
        /// <param name="propertyExpression">一个属性访问表达式。</param>
        /// <param name="valueExpression">一个值表达式。</param>
        /// <param name="updateSettersBuilder"></param>
        public static SetPropertyCalls<TSource> SetPropertyIf<TSource, TProperty>(
            this SetPropertyCalls<TSource> updateSettersBuilder,
            Func<TProperty, bool> condition,
            Func<TSource, TProperty> propertyExpression,
            TProperty valueExpression)
        {
            if (!condition(valueExpression))
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, valueExpression);
            return updateSettersBuilder;
        }

#elif NET10_0_OR_GREATER
        /// <summary>
        /// 指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值。
        /// </summary>
        /// <typeparam name="TProperty">属性的类型。</typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="condition">条件</param>
        /// <param name="propertyExpression">一个属性访问表达式。</param>
        /// <param name="valueExpression">一个值表达式。</param>
        /// <param name="updateSettersBuilder"></param>
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
        /// 指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="propertyExpression">一个属性访问表达式。</param>
        /// <param name="valueExpression">一个值表达式。</param>
        /// <param name="updateSettersBuilder"></param>
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
        /// 指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值。
        /// </summary>
        /// <typeparam name="TProperty">属性的类型。</typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="propertyExpression">一个属性访问表达式。</param>
        /// <param name="valueExpression">一个值表达式。</param>
        /// <param name="updateSettersBuilder"></param>
        public static UpdateSettersBuilder<TSource> SetPropertyIfNotNull<TSource, TProperty>(
            this UpdateSettersBuilder<TSource> updateSettersBuilder,
            Expression<Func<TSource, TProperty>> propertyExpression,
            TProperty valueExpression)
        {
            if (valueExpression is null)
                return updateSettersBuilder;

            updateSettersBuilder.SetProperty(propertyExpression, Expression.Constant(valueExpression, typeof(TProperty)));
            return updateSettersBuilder;
        }

        /// <summary>
        /// 指定一个属性及在 ExecuteUpdate 方法中应更新为的对应值。
        /// </summary>
        /// <typeparam name="TProperty">属性的类型。</typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="condition">条件</param>
        /// <param name="propertyExpression">一个属性访问表达式。</param>
        /// <param name="valueExpression">一个值表达式。</param>
        /// <param name="updateSettersBuilder"></param>
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

#endif
    }
}