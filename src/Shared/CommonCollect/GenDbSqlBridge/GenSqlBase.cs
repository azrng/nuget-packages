using CommonCollect.GenDbSqlBridge.Model;
using System.Text;

namespace CommonCollect.GenDbSqlBridge
{
    public abstract class GenSqlBase
    {
        public StructDbTypeEnum OriginDbType { get; set; }

        public GenSqlBase(StructDbTypeEnum originDbType)
        {
            OriginDbType = originDbType;
        }

        public abstract StructDbTypeEnum CurrDbType { get; }

        /// <summary>
        /// 生成Add schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public abstract StringBuilder AddSchemaSql(params string[] schema);

        /// <summary>
        /// 生成Add schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public abstract StringBuilder RemoveSchemaSql(params string[] schema);

        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="tableStructInfos"></param>
        /// <returns></returns>
        public abstract StringBuilder AddTableSql(List<TableStructInfo> tableStructInfos);

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableStructInfos"></param>
        /// <returns></returns>
        public abstract StringBuilder RemoveTableSql(List<RemoveTableInfo> tableStructInfos);

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="columnBaseBos"></param>
        /// <returns></returns>
        public abstract StringBuilder AddColumnSql(List<ColumnBaseBo> columnBaseBos);

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="columnInfos"></param>
        /// <returns></returns>
        public abstract StringBuilder RemoveColumnSql(List<RemoveColumnInfo> columnInfos);

        /// <summary>
        /// 转换列类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string ConvertColumnType(string type, string length = "-1")
        {
            if (type.IsNullOrWhiteSpace())
                return string.Empty;

            if (OriginDbType == StructDbTypeEnum.MYSQL && CurrDbType == StructDbTypeEnum.PG)
                return ConvertMySqlToPgSqlType(type, length);

            return string.Empty;
        }

        /// <summary>
        /// 转换mysql 到 pgsql
        /// </summary>
        /// <param name="mysqlType"></param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        private static string ConvertMySqlToPgSqlType(string mysqlType, string length)
        {
            // 建立MySQL到PostgreSQL类型映射关系
            var typeMapping = new Dictionary<string, string>
                              {
                                  { "int", "integer" },
                                  { "tinyint", "smallint" },
                                  { "smallint", "smallint" },
                                  { "mediumint", "integer" },
                                  { "bigint", "bigint" },
                                  { "float", "real" },
                                  { "double", "numeric" },
                                  { "real", "numeric" },
                                  { "decimal", "numeric" },
                                  { "char", "char" },
                                  { "varchar", "varchar" },
                                  { "tinytext", "text" },
                                  { "text", "text" },
                                  { "mediumtext", "text" },
                                  { "longtext", "text" },
                                  { "date", "date" },
                                  { "datetime", "timestamp" },
                                  { "timestamp", "timestamp" },
                                  { "time", "time" },
                                  { "year", "integer" } // MySQL的YEAR类型在PostgreSQL中没有直接对应，这里简单处理为integer
                              };

            // 尝试获取对应的PostgreSQL类型，如果找不到则返回原始MySQL类型
            if (typeMapping.TryGetValue(mysqlType, out var pgsqlType))
            {
                // 特殊处理
                if (mysqlType == "tinyint" && length == "1")
                    return "bool";
                if (pgsqlType == "text" || pgsqlType == "smallint")
                    return pgsqlType;
                if (length.IsNotNullOrWhiteSpace() && length != "-1")
                {
                    return $"{pgsqlType}({length})";
                }

                return pgsqlType;
            }
            else
            {
                // 默认返回MySQL类型本身（需要根据实际情况可能会有更复杂的映射需求）
                return mysqlType;
            }
        }
    }
}