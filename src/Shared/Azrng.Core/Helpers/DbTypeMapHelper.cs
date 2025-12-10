namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 数据库类型映射
    /// </summary>
    public class DbTypeMapHelper
    {
        /// <summary>
        /// MySQL映射c#类型
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public static string MySqlMapCsharpType(string dbType, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbType)) return dbType;
            dbType = dbType.ToLowerInvariant();
            var csharpType = "object";
            switch (dbType)
            {
                case "int":
                case "mediumint":
                case "integer":
                    csharpType = isNullable ? "int?" : "int";
                    break;
                case "varchar":
                case "text":
                case "char":
                case "enum":
                case "mediumtext":
                case "tinytext":
                case "longtext":
                    csharpType = "string";
                    break;
                case "tinyint":
                    csharpType = isNullable ? "byte?" : "byte";
                    break;
                case "smallint":
                    csharpType = isNullable ? "short?" : "short";
                    break;
                case "bigint":
                    csharpType = isNullable ? "long?" : "long";
                    break;
                case "bit":
                    csharpType = isNullable ? "bool?" : "bool";
                    break;
                case "real":
                case "double":
                    csharpType = isNullable ? "double?" : "double";
                    break;
                case "float":
                    csharpType = isNullable ? "float?" : "float";
                    break;
                case "decimal":
                case "numeric":
                    csharpType = isNullable ? "decimal?" : "decimal";
                    break;
                case "year":
                    csharpType = isNullable ? "int?" : "int";
                    break;
                case "datetime":
                case "timestamp":
                case "date":
                case "time":
                    csharpType = isNullable ? "DateTime?" : "DateTime";
                    break;
                case "blob":
                case "longblob":
                case "tinyblob":
                case "varbinary":
                case "binary":
                case "multipoint":
                case "geometry":
                case "multilinestring":
                case "polygon":
                case "mediumblob":
                    csharpType = "byteArray";
                    break;
            }

            return csharpType;
        }

        /// <summary>
        /// Oracle映射c#类型
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public static string OracleMapCsharpType(string dbType, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbType)) return dbType;
            dbType = dbType.ToLowerInvariant();
            string csharpType;
            switch (dbType)
            {
                case "int":
                case "integer":
                case "interval year to  month":
                case "interval day to  second":
                case "number":
                    csharpType = isNullable ? "int?" : "int";
                    break;
                case "decimal":
                    csharpType = isNullable ? "decimal?" : "decimal";
                    break;
                case "varchar":
                case "varchar2":
                case "nvarchar2":
                case "char":
                case "nchar":
                case "clob":
                case "long":
                case "nclob":
                case "rowid":
                    csharpType = "string";
                    break;
                case "date":
                case "timestamp":
                case "timestamp with local time zone":
                case "timestamp with time zone":
                    csharpType = isNullable ? "DateTime?" : "DateTime";
                    break;
                default:
                    csharpType = "object";
                    break;
            }

            return csharpType;
        }

        /// <summary>
        /// Postgre映射c#类型
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public static string PostgreMapCsharpType(string dbType, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbType)) return dbType;
            dbType = dbType.ToLowerInvariant();
            var csharpType = "object";
            switch (dbType)
            {
                case "int2":
                case "smallint":
                    csharpType = isNullable ? "short?" : "short";
                    break;
                case "int4":
                case "double precision":
                case "integer":
                    csharpType = isNullable ? "int?" : "int";
                    break;
                case "int8":
                case "bigint":
                    csharpType = isNullable ? "long?" : "long";
                    break;
                case "float4":
                case "real":
                    csharpType = isNullable ? "float?" : "float";
                    break;
                case "float8":
                    csharpType = isNullable ? "double?" : "double";
                    break;
                case "numeric":
                case "decimal":
                case "path":
                case "point":
                case "interval":
                case "lseg":
                case "macaddr":
                case "money":
                case "polygon":
                    csharpType = isNullable ? "decimal?" : "decimal";
                    break;
                case "boolean":
                case "bool":
                case "box":
                case "bytea":
                    csharpType = isNullable ? "bool?" : "bool";
                    break;
                case "varchar":
                case "character varying":
                case "geometry":
                case "name":
                case "text":
                case "char":
                case "character":
                case "cidr":
                case "circle":
                case "tsquery":
                case "tsvector":
                case "xml":
                case "json":
                case "txid_snapshot":
                    csharpType = "string";
                    break;
                case "uuid":
                    csharpType = isNullable ? "Guid?" : "Guid";
                    break;
                case "timestamp":
                case "timestamp with time zone":
                case "timestamptz":
                case "timestamp without time zone":
                case "date":
                case "time":
                case "time with time zone":
                case "timetz":
                case "time without time zone":
                    csharpType = isNullable ? "DateTime?" : "DateTime";
                    break;
                case "bit":
                case "bit varying":
                    csharpType = isNullable ? "byteArray?" : "byteArray";
                    break;
                case "varbit":
                    csharpType = isNullable ? "byte?" : "byte";
                    break;
                case "public.geometry":
                case "inet":
                    csharpType = "object";
                    break;
            }

            return csharpType;
        }

        /// <summary>
        /// SqlServer映射c#类型
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public static string SqlServerMapCsharpType(string dbType, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbType)) return dbType;
            dbType = dbType.ToLowerInvariant();
            string csharpType;
            switch (dbType)
            {
                case "bigint":
                    csharpType = isNullable ? "long?" : "long";
                    break;
                case "binary":
                    csharpType = "byte[]";
                    break;
                case "bit":
                    csharpType = isNullable ? "bool?" : "bool";
                    break;
                case "char":
                    csharpType = "string";
                    break;
                case "date":
                    csharpType = isNullable ? "DateTime?" : "DateTime";
                    break;
                case "datetime":
                    csharpType = isNullable ? "DateTime?" : "DateTime";
                    break;
                case "datetime2":
                    csharpType = isNullable ? "DateTime?" : "DateTime";
                    break;
                case "datetimeoffset":
                    csharpType = isNullable ? "DateTimeOffset?" : "DateTimeOffset";
                    break;
                case "decimal":
                    csharpType = isNullable ? "decimal?" : "decimal";
                    break;
                case "float":
                    csharpType = isNullable ? "double?" : "double";
                    break;
                case "image":
                    csharpType = "byte[]";
                    break;
                case "int":
                    csharpType = isNullable ? "int?" : "int";
                    break;
                case "money":
                    csharpType = isNullable ? "decimal?" : "decimal";
                    break;
                case "nchar":
                    csharpType = "string";
                    break;
                case "ntext":
                    csharpType = "string";
                    break;
                case "numeric":
                    csharpType = isNullable ? "decimal?" : "decimal";
                    break;
                case "nvarchar":
                    csharpType = "string";
                    break;
                case "real":
                    csharpType = isNullable ? "Single?" : "Single";
                    break;
                case "smalldatetime":
                    csharpType = isNullable ? "DateTime?" : "DateTime";
                    break;
                case "smallint":
                    csharpType = isNullable ? "short?" : "short";
                    break;
                case "smallmoney":
                    csharpType = isNullable ? "decimal?" : "decimal";
                    break;
                case "sql_variant":
                    csharpType = "object";
                    break;
                case "sysname":
                    csharpType = "object";
                    break;
                case "text":
                    csharpType = "string";
                    break;
                case "time":
                    csharpType = isNullable ? "TimeSpan?" : "TimeSpan";
                    break;
                case "timestamp":
                    csharpType = "byte[]";
                    break;
                case "tinyint":
                    csharpType = isNullable ? "byte?" : "byte";
                    break;
                case "uniqueidentifier":
                    csharpType = isNullable ? "Guid?" : "Guid";
                    break;
                case "varbinary":
                    csharpType = "byte[]";
                    break;
                case "varchar":
                    csharpType = "string";
                    break;
                case "xml":
                    csharpType = "string";
                    break;
                default:
                    csharpType = "object";
                    break;
            }

            return csharpType;
        }

        /// <summary>
        /// Sqlite映射c#类型
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public static string SqliteMapCsharpType(string dbType, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbType)) return dbType;
            dbType = dbType.ToLowerInvariant();
            var csharpType = "object";
            switch (dbType)
            {
                case "integer":
                case "int":
                case "int32":
                case "integer32":
                case "number":
                    csharpType = isNullable ? "int?" : "int";
                    break;
                case "varchar":
                case "varchar2":
                case "nvarchar":
                case "nvarchar2":
                case "text":
                case "ntext":
                case "blob_text":
                case "char":
                case "nchar":
                case "num":
                case "currency":
                case "datetext":
                case "word":
                case "graphic":
                    csharpType = "string";
                    break;
                case "tinyint":
                case "unsignedinteger8":
                    csharpType = isNullable ? "byte?" : "byte";
                    break;
                case "smallint":
                case "int16":
                    csharpType = isNullable ? "short?" : "short";
                    break;
                case "bigint":
                case "int64":
                case "long":
                case "integer64":
                    csharpType = isNullable ? "long?" : "long";
                    break;
                case "bit":
                case "bool":
                case "boolean":
                    csharpType = isNullable ? "bool?" : "bool";
                    break;
                case "real":
                case "double":
                    csharpType = isNullable ? "double?" : "double";
                    break;
                case "float":
                    csharpType = isNullable ? "float?" : "float";
                    break;
                case "decimal":
                case "dec":
                case "numeric":
                case "money":
                case "smallmoney":
                    csharpType = isNullable ? "decimal?" : "decimal";
                    break;
                case "datetime":
                case "timestamp":
                case "date":
                case "time":
                    csharpType = isNullable ? "DateTime?" : "DateTime";
                    break;
                case "blob":
                case "clob":
                case "raw":
                case "oleobject":
                case "binary":
                case "photo":
                case "picture":
                    csharpType = isNullable ? "byteArray?" : "byteArray";
                    break;
                case "uniqueidentifier":
                    csharpType = isNullable ? "Guid?" : "Guid";
                    break;
            }

            return csharpType;
        }
    }
}