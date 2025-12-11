using CommonCollect.InfluxDb.Attributes;
using InfluxData.Net.InfluxDb.ClientModules;
using InfluxData.Net.InfluxDb.Models;
using InfluxData.Net.InfluxDb.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommonCollect.InfluxDb.Extensions
{
    public static class BasicClientModuleExtensions
    {
        public static async Task AddAsync<TEntity>(this IBasicClientModule basicClientModule, TEntity entity, string dbName = null, string retentionPolicy = null) where TEntity : class, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            Type type = entity.GetType();
            Point val = new Point
            {
                Name = type.Name,
                Tags = new Dictionary<string, object>(),
                Fields = new Dictionary<string, object>()
            };
            Point val2 = val;
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.GetCustomAttributes(inherit: false).OfType<InfluxTagAttribute>().FirstOrDefault() != null)
                {
                    val2.Tags.Add(propertyInfo.Name, propertyInfo.GetValue(entity) ?? string.Empty);
                }
                else
                {
                    val2.Fields.Add(propertyInfo.Name, propertyInfo.GetValue(entity) ?? string.Empty);
                }
            }
            await basicClientModule.WriteAsync(val2, dbName, retentionPolicy, "ms");
        }

        public static async Task<List<TEntity>> GetListAsync<TEntity>(this IBasicClientModule basicClientModule, string querySql) where TEntity : class, new()
        {
            if (string.IsNullOrWhiteSpace(querySql))
            {
                throw new ArgumentNullException("querySql");
            }
            List<TEntity> result = new List<TEntity>();
            Type type = typeof(TEntity);
            Serie val = (await basicClientModule.QueryAsync(querySql, null, null, null)).FirstOrDefault();
            if (val == null)
            {
                return result;
            }
            foreach (IList<object> value3 in val.Values)
            {
                TEntity val2 = new TEntity();
                for (int i = 0; i < val.Columns.Count; i++)
                {
                    string text = val.Columns[i];
                    if (text != "time")
                    {
                        object value = value3[i];
                        PropertyInfo property = type.GetProperty(text);
                        object value2 = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(val2, value2);
                    }
                }
                result.Add(val2);
            }
            return result;
        }

        public static async Task<TEntity> GetFirstOrDefaultAsync<TEntity>(this IBasicClientModule basicClientModule, string querySql) where TEntity : class, new()
        {
            if (string.IsNullOrWhiteSpace(querySql))
            {
                throw new ArgumentNullException("querySql");
            }
            TEntity result = new TEntity();
            Type type = typeof(TEntity);
            Serie val = (await basicClientModule.QueryAsync(querySql, null, null, null)).FirstOrDefault();
            if (val == null)
            {
                return result;
            }
            foreach (IList<object> value3 in val.Values)
            {
                TEntity val2 = new TEntity();
                for (int i = 0; i < val.Columns.Count; i++)
                {
                    string text = val.Columns[i];
                    if (!(text == "time"))
                    {
                        object value = value3[i];
                        PropertyInfo property = type.GetProperty(text);
                        object value2 = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(val2, value2);
                    }
                }
                if (val2 != null)
                {
                    result = val2;
                }
            }
            return result;
        }

        public static async Task<(int, List<TEntity>)> GetPageListAsync<TEntity>(this IBasicClientModule basicClientModule, string querySql) where TEntity : class, new()
        {
            int totalCount = 0;
            List<TEntity> result = null;
            List<string> queries = null;
            Type type = null;
            Initialization();
            PropertyInfo propertyInfo = type.GetProperties().FirstOrDefault((r) => !r.GetCustomAttributes(inherit: false).OfType<InfluxTagAttribute>().Any());
            if (propertyInfo == null)
            {
                throw new InvalidOperationException("无法查询数据数量，请检查表内是否存在可查询字段");
            }
            string item = "SELECT COUNT(\"" + propertyInfo.Name + "\") FROM " + type.Name;
            queries.Add(item);
            IEnumerable<Serie> source = await basicClientModule.QueryAsync(queries, null, null, null);
            Serie val = source.FirstOrDefault();
            if (val == null)
            {
                return (totalCount, result);
            }
            Serie val2 = source.LastOrDefault();
            if (val2 != null)
            {
                totalCount = Convert.ToInt32(val2.Values.FirstOrDefault().LastOrDefault());
            }
            foreach (IList<object> value3 in val.Values)
            {
                TEntity val3 = new TEntity();
                for (int i = 0; i < val.Columns.Count; i++)
                {
                    string text = val.Columns[i];
                    if (text != "time")
                    {
                        object value = value3[i];
                        PropertyInfo property = type.GetProperty(text);
                        object value2 = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(val3, value2);
                    }
                }
                result.Add(val3);
            }
            return (totalCount, result);
            void Initialization()
            {
                result = new List<TEntity>();
                queries = new List<string>
                {
                    querySql
                };
                type = typeof(TEntity);
            }
        }
    }
}