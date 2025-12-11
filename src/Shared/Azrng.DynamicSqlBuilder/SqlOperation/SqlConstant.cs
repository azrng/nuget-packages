namespace ConsoleApp.Models.DynamicSql.SqlOperation
{
    /// <summary>
    /// Sql操作符常量
    /// </summary>
    public static class SqlConstant
    {
        /// <summary>
        /// 逻辑操作符 and
        /// </summary>
        public const string LogicalOperatorAnd = "AND";

        /// <summary>
        /// 操作符 等于
        /// </summary>
        public const string MatchOperatorEqual = "=";

        /// <summary>
        /// 操作符 不等于
        /// </summary>
        public const string MatchOperatorNotEqual = "<>";

        /// <summary>
        /// 操作符 大于等于
        /// </summary>
        public const string MatchOperatorGreaterThanEqual = ">=";

        /// <summary>
        /// 操作符 小于等于
        /// </summary>
        public const string MatchOperatorLessThanEqual = "<=";

        /// <summary>
        /// 操作符 like
        /// </summary>
        public const string MatchOperatorlike = "LIKE";

        /// <summary>
        /// 操作符 like
        /// </summary>
        public const string MatchOperatorNotlike = " NOT LIKE";

        /// <summary>
        /// 操作符 between
        /// </summary>
        public const string MatchOperatorBetween = "BETWEEN";

        /// <summary>
        /// 操作符 in
        /// </summary>
        public const string MatchOperatorIn = "IN";

        /// <summary>
        /// 操作符 in
        /// </summary>
        public const string MatchOperatorNotIn = "Not IN";

        /// <summary>
        /// 操作符 and
        /// </summary>
        public const string MatchOperatorAnd = "AND";

        /// <summary>
        /// 排序操作 order by
        /// </summary>
        public const string SortOperatorOrderBy = " ORDER BY ";

        /// <summary>
        /// 分页操作 模板
        /// </summary>
        public const string PaginationOperatorTemplate = " LIMIT {0} OFFSET {1} ";

        /// <summary>
        /// 升序
        /// </summary>
        public const string OrderByASC = " ASC ";

        /// <summary>
        /// 降序
        /// </summary>
        public const string OrderByDESC = " DESC ";

        /// <summary>
        /// row_number 窗口函数 模板
        /// </summary>
        public const string RowNumberWindowFunctionTemplate = " row_number() over(order by {0} ) as rowNumber ";
    }
}
