using AutoMapper;
using System.Collections;
using System.Collections.Generic;

namespace CommonCollect.Extension
{
    public static class AutoMapperExtensions
    {
        /// <summary>
        ///  类型映射
        /// </summary>
        public static T MapTo<T>(this object obj)
        {
            if (obj == null) return default;

            var config = new MapperConfiguration(cfg => cfg.CreateMap(obj.GetType(), typeof(T)));
            var mapper = config.CreateMapper();
            return mapper.Map<T>(obj);
        }

        /// <summary>
        /// 集合列表类型映射
        /// </summary>
        public static List<TDestination> MapToList<TDestination>(this IEnumerable source)
        {
            var sourceType = source.GetType().GetGenericArguments()[0]; //获取枚举的成员类型
            var config = new MapperConfiguration(cfg => cfg.CreateMap(sourceType, typeof(TDestination)));
            var mapper = config.CreateMapper();

            return mapper.Map<List<TDestination>>(source);
        }

        /// <summary>
        /// 集合列表类型映射
        /// </summary>
        public static List<TDestination> MapToList<TSource, TDestination>(this IEnumerable<TSource> source)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(TSource), typeof(TDestination)));
            var mapper = config.CreateMapper();

            return mapper.Map<List<TDestination>>(source);
        }
    }
}