using Azrng.Core.CommonDto;
using Azrng.Core.Requests;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azrng.EFCore.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalNumber"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<T>> ToPageListAsync<T>(this IQueryable<T> queryable,
                                                             int pageIndex,
                                                             int pageSize,
                                                             RefAsync<int> totalNumber) where T : class
        {
            var refAsync = totalNumber;
            var num = await queryable.CountAsync();
            refAsync.Value = num;
            var rows = await queryable.Skip(pageSize * (pageIndex - 1))
                                      .Take(pageSize)
                                      .ToListAsync();
            return rows;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<List<T>> ToPageListAsync<T>(this IQueryable<T> queryable,
                                                       int pageIndex,
                                                       int pageSize) where T : class
        {
            return queryable.Skip(pageSize * (pageIndex - 1))
                            .Take(pageSize)
                            .ToListAsync();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="pageContent"></param>
        /// <param name="totalNumber"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<T>> ToPageListAsync<T>(this IQueryable<T> queryable,
                                                             GetPageRequest pageContent,
                                                             RefAsync<int> totalNumber) where T : class
        {
            var refAsync = totalNumber;
            var num = await queryable.CountAsync();
            refAsync.Value = num;
            var rows = await queryable.Skip(pageContent.PageSize * (pageContent.PageIndex - 1))
                                      .Take(pageContent.PageSize)
                                      .ToListAsync();
            return rows;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="pageContent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<List<T>> ToPageListAsync<T>(this IQueryable<T> queryable,
                                                       GetPageRequest pageContent) where T : class
        {
            return queryable.Skip(pageContent.PageSize * (pageContent.PageIndex - 1))
                            .Take(pageContent.PageSize)
                            .ToListAsync();
        }

        // /// <summary>
        // /// 左连接查询
        // /// </summary>
        // /// <param name="outer"></param>
        // /// <param name="inner"></param>
        // /// <param name="outerKeySelector"></param>
        // /// <param name="innerKeySelector"></param>
        // /// <param name="resultSelector"></param>
        // /// <typeparam name="TOuter"></typeparam>
        // /// <typeparam name="TInner"></typeparam>
        // /// <typeparam name="TResult"></typeparam>
        // /// <typeparam name="TKey"></typeparam>
        // /// <returns></returns>
        // public static async Task<List<TResult>> LeftJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer,
        //                                                                                 IQueryable<TInner> inner,
        //                                                                                 Expression<Func<TOuter, TKey>> outerKeySelector,
        //                                                                                 Expression<Func<TInner, TKey>> innerKeySelector,
        //                                                                                 Expression<Func<TOuter, TInner, TResult>>
        //                                                                                     resultSelector)
        // {
        //     var aareturn = outer
        //                    .GroupJoin(inner, outerKeySelector, innerKeySelector,
        //                        (outerObj, inners) => new { outerObj, inners })
        //                    .SelectMany(x => x.inners.DefaultIfEmpty(), resultSelector)
        //                    .ToListAsync();
        //
        //     return await outer
        //                  .GroupJoin(inner, outerKeySelector, innerKeySelector,
        //                      (outerObj, inners) => new { outerObj, inners = inners.DefaultIfEmpty() })
        //                  .SelectMany(a => a.inners.Select(innerObj => resultSelector(a.outerObj, innerObj)))
        //                  .ToListAsync();
        // }
    }
}