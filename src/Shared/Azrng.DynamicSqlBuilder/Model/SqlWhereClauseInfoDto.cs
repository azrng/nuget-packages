namespace Azrng.DynamicSqlBuilder.Model
{
    /// <summary>
    /// Sql where 子句 信息
    /// </summary>
    public class SqlWhereClauseInfoDto
    {
        public SqlWhereClauseInfoDto()
        {
            ValueType = typeof(string);
        }

        public SqlWhereClauseInfoDto(string fieldName, List<FieldValueInfoDto> fieldValueInfos,
                                     MatchOperator matchOperator = MatchOperator.Equal,
                                     string logicalOperator = "And", Type valueType = null,
                                     IEnumerable<SqlWhereClauseInfoDto> nestedChildren = null)
        {
            MatchOperator = matchOperator;
            FieldName = fieldName;
            FieldValueInfos = fieldValueInfos;
            LogicalOperator = logicalOperator;
            NestedChildrens = nestedChildren;
            ValueType = valueType ?? typeof(string);
        }

        /// <summary>
        /// 匹配操作符  如 BETWEEN,IN,LIKE...etc
        /// </summary>
        public MatchOperator MatchOperator { get; set; }

        /// <summary>
        /// 字段名
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 字段值
        /// </summary>
        public List<FieldValueInfoDto> FieldValueInfos { get; set; }

        /// <summary>
        /// 逻辑运算符 如 AND,OR
        /// </summary>
        public string LogicalOperator { get; set; } = "And";

        /// <summary>
        /// 嵌套条件
        /// </summary>
        public IEnumerable<SqlWhereClauseInfoDto> NestedChildrens { get; set; }

        /// <summary>
        /// 值的实际类型
        /// </summary>
        public Type ValueType { get; set; }
    }
}