using Azrng.Core.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Azrng.Core.Extension
{
    /// <summary>
    ///  IQueryable扩展
    /// </summary>
    public static class QueryableExtensions
    {
        #region 查询

        /// <summary>
        /// 查询映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Tm"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static IQueryable<Tm> SelectMapper<T, Tm>(this IQueryable<T> queryable)
            where T : class
        {
            var parameter = Expression.Parameter(typeof(T), "t");
            var newExpression = Expression.New(typeof(Tm));

            var mapperType = typeof(T).GetProperties();

            var listBinding = new List<MemberBinding>();
            foreach (var item in typeof(Tm).GetProperties())
            {
                if (mapperType.All(t => t.Name != item.Name))
                {
                    continue;
                }

                var mem = Expression.Property(parameter, item.Name); // t.name
                var members = typeof(Tm).GetMember(item.Name);
                if (members.Length == 0)
                {
                    continue;
                }

                var member = members[0];
                MemberBinding memBinding = Expression.Bind(member, mem); // 这里传mem是用t.name给他赋值
                listBinding.Add(memBinding);
            }

            var memberExp = Expression.MemberInit(newExpression, listBinding);
            var selectExpression = Expression.Lambda<Func<T, Tm>>(memberExp, new ParameterExpression[]
                                                                             {
                                                                                 parameter
                                                                             });
            return queryable.Select(selectExpression);
        }

        #endregion

        #region 排序

        /// <summary>
        /// 根据指定列进行排序排序
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="query"></param>
        /// <param name="keySelector"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> query,
                                                                 Expression<Func<TSource, TKey>> keySelector,
                                                                 bool isAsc) where TSource : class
        {
            return query.OrderBy(keySelector, isAsc ? SortEnum.Asc : SortEnum.Desc);
        }

        /// <summary>
        /// 根据指定列进行排序排序
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="query"></param>
        /// <param name="keySelector"></param>
        /// <param name="sortEnum"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> query,
                                                                 Expression<Func<TSource, TKey>> keySelector,
                                                                 SortEnum sortEnum) where TSource : class
        {
            if (query == null || keySelector == null)
                throw new ArgumentNullException(nameof(query));

            return sortEnum == SortEnum.Asc ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderContent"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query,
                                                           params SortContent[] orderContent)
            where TEntity : class
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return orderContent.Length == 0
                ? query
                : orderContent.Aggregate(query, (current, item) => current.OrderBy(item));
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderContent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query,
                                                           SortContent orderContent)
            where TEntity : class
        {
            if (query == null || orderContent == null || string.IsNullOrWhiteSpace(orderContent.SortName))
                throw new ArgumentNullException(nameof(query));

            //说明：使用包System.Linq.Dynamic.Core  写法 orderby("time asc")
            //return query.OrderBy($"{orderContent.SortName} {orderContent.Sort}");
            // 这个排序方案是不行的
            //return orderContent.Sort == SortEnum.Asc ? query.OrderBy(t => orderContent.SortName) : query.OrderByDescending(t => orderContent.SortName);

            // 自己构建表达式树的方式去排序
            return query.OrderBy(orderContent.SortName, orderContent.Sort == SortEnum.Asc);
        }

        /// <summary>
        /// 根据string名称进行排序
        /// </summary>
        /// <typeparam name="T">泛型列</typeparam>
        /// <param name="queryable">查询queryable</param>
        /// <param name="sortField">排序列</param>
        /// <param name="isAsc">true正序 false倒序</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, string sortField,
                                               bool isAsc = true)
            where T : class
        {
            var parameter = Expression.Parameter(typeof(T), sortField);
            var property = typeof(T).GetProperty(sortField);
            if (property == null)
                throw new ArgumentNullException($"无效的属性 {sortField}");

            var memberExpression = Expression.Property(parameter, property);
            var orderByExpression = Expression.Lambda(memberExpression, new ParameterExpression[]
                                                                        {
                                                                            parameter
                                                                        });

            var orderMethod = isAsc ? "OrderBy" : "OrderByDescending";
            var resultExpression = Expression.Call(typeof(Queryable), orderMethod,
                new[]
                {
                    queryable.ElementType,
                    property.PropertyType
                }, queryable.Expression,
                Expression.Quote(orderByExpression));

            return queryable.Provider.CreateQuery<T>(resultExpression);
        }

        /// <summary>
        /// 根据多个string名称排序(多字段)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">数据源</param>
        /// <param name="orderParams">排序参数</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable,
                                               params FiledOrderParam[] orderParams)
            where T : class
        {
            var parameter = Expression.Parameter(typeof(T), "t");
            if (orderParams.Length <= 0)
            {
                return queryable;
            }

            for (var i = 0; i < orderParams.Length; i++)
            {
                var property = typeof(T).GetProperty(orderParams[i].PropertyName);
                if (property == null)
                {
                    continue;
                }

                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExpr = Expression.Lambda(propertyAccess, parameter);
                var methodName = i > 0
                    ? orderParams[i].IsAsc ? "ThenBy" : "ThenByDescending"
                    : orderParams[i].IsAsc
                        ? "OrderBy"
                        : "OrderByDescending";
                var resultExp = Expression.Call(typeof(Queryable), methodName,
                    new[]
                    {
                        queryable.ElementType,
                        property.PropertyType
                    },
                    queryable.Expression, Expression.Quote(orderByExpr));
                queryable = queryable.Provider.CreateQuery<T>(resultExp);
            }

            return queryable;
        }

        #endregion

        #region 分页

        /// <summary>
        /// 分页方法
        /// </summary>
        /// <typeparam name="TEntity">返回的对象</typeparam>
        /// <param name="query">IQueryable</param>
        /// <param name="pageContent">分页参数</param>
        /// <returns></returns>
        public static IQueryable<TEntity> PagedBy<TEntity>(this IQueryable<TEntity> query,
                                                           GetPageRequest pageContent)
            where TEntity : class
        {
            if (query == null || pageContent == null)
                throw new ArgumentNullException(nameof(query));

            return query.Skip((pageContent.PageIndex - 1) * pageContent.PageSize).Take(pageContent.PageSize);
        }

        /// <summary>
        /// 分页方法
        /// </summary>
        /// <typeparam name="TEntity">返回的对象</typeparam>
        /// <param name="query">IQueryable</param>
        /// <param name="pageContent">分页参数</param>
        /// <param name="totalCount">总条数</param>
        /// <returns></returns>
        public static IQueryable<TEntity> PagedBy<TEntity>(this IQueryable<TEntity> query,
                                                           GetPageRequest pageContent,
                                                           out int totalCount)
            where TEntity : class
        {
            if (query == null || pageContent == null)
                throw new ArgumentNullException(nameof(query));

            totalCount = query.Count();

            return query.Skip((pageContent.PageIndex - 1) * pageContent.PageSize).Take(pageContent.PageSize);
        }

        /// <summary>
        /// 分页方法
        /// </summary>
        /// <typeparam name="TEntity">返回的对象</typeparam>
        /// <param name="query">IQueryable</param>
        /// <param name="pageIndex">页数</param>
        /// <param name="pageSize">页码</param>
        /// <returns></returns>
        public static IQueryable<TEntity> PagedBy<TEntity>(this IQueryable<TEntity> query, int pageIndex = 1,
                                                           int pageSize = 10) where TEntity : class
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 分页方法
        /// </summary>
        /// <typeparam name="TEntity">返回的对象</typeparam>
        /// <param name="query">IQueryable</param>
        /// <param name="pageIndex">页数</param>
        /// <param name="pageSize">页码</param>
        /// <param name="totalCount">总条数</param>
        /// <returns></returns>
        public static IQueryable<TEntity> PagedBy<TEntity>(this IQueryable<TEntity> query, int pageIndex,
                                                           int pageSize,
                                                           out int totalCount) where TEntity : class
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            totalCount = query.Count();

            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        #endregion

        #region where

        /// <summary>
        /// 等于筛选
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="whereField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IQueryable<T> EqualWhere<T>(this IQueryable<T> queryable, string whereField,
                                                  object value)
            where T : class
        {
            return queryable.Where(whereField, value);
        }

        /// <summary>
        /// 小于筛选
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="whereField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IQueryable<T> LessWhere<T>(this IQueryable<T> queryable, string whereField,
                                                 object value)
            where T : class
        {
            return queryable.Where(whereField, value, 1);
        }

        /// <summary>
        /// 大于筛选
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="whereField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IQueryable<T> GreaterWhere<T>(this IQueryable<T> queryable, string whereField,
                                                    object value)
            where T : class
        {
            return queryable.Where(whereField, value, 1);
        }

        /// <summary>
        /// 模拟EFCore的OR查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">queryable</param>
        /// <param name="predicates">多个 条件</param>
        /// <returns></returns>
        public static IQueryable<T> WhereAny<T>(this IQueryable<T> queryable,
                                                params Expression<Func<T, bool>>[] predicates)
            where T : class
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            if (predicates == null)
                throw new ArgumentNullException(nameof(predicates));
            switch (predicates.Length)
            {
                case 0:
                    return queryable.Where(x => false);
                case 1:
                    return queryable.Where(predicates[0]);
            }

            var parameterExpression = Expression.Parameter(typeof(T), "x");
            var expression = (Expression)Expression.Invoke(predicates[0], parameterExpression);
            for (var index = 1; index < predicates.Length; ++index)
                expression = Expression.OrElse(expression, Expression.Invoke(predicates[index], parameterExpression));
            var predicate = Expression.Lambda<Func<T, bool>>(expression, parameterExpression);
            return queryable.Where(predicate);
        }

        #endregion

        #region 查询总条数

        /// <summary>
        /// 查询总条数
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="totalCount">总条数</param>
        /// <returns></returns>
        public static IQueryable<TEntity> CountBy<TEntity>(this IQueryable<TEntity> queryable,
                                                           out int totalCount)
            where TEntity : class
        {
            totalCount = queryable.Count();
            return queryable;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// where查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">queryable</param>
        /// <param name="whereField">where条件的列</param>
        /// <param name="value">值</param>
        /// <param name="type">类型 1小于 2小于等于 3大于 4大于等于 其他等于</param>
        /// <returns></returns>
        private static IQueryable<T> Where<T>(this IQueryable<T> queryable, string whereField, object value,
                                              int type = 0)
            where T : class
        {
            var paramExp = Expression.Parameter(typeof(T), "t");

            //因为这个Property里面已经包含属性校验的功能，所以不用再另外写了
            var memberExp = Expression.Property(paramExp, whereField);

            //值表达式
            var valueExp = Expression.Constant(value);
            var exp = type switch
            {
                //小于
                1 => Expression.LessThan(memberExp, valueExp),

                //小于等于
                2 => Expression.LessThanOrEqual(memberExp, valueExp),

                //大于
                3 => Expression.GreaterThan(memberExp, valueExp),

                //大于等于
                4 => Expression.GreaterThanOrEqual(memberExp, valueExp),

                //等于
                _ => Expression.Equal(memberExp, valueExp),
            };
            var lambda = Expression.Lambda<Func<T, bool>>(exp, paramExp);

            return queryable.Where(lambda);
        }

        #endregion
    }
}