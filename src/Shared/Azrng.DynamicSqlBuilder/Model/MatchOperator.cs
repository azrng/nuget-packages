namespace Azrng.DynamicSqlBuilder.Model;

public enum MatchOperator
{
    /// <summary>
    /// 等于操作符 =
    /// </summary>
    Equal,

    /// <summary>
    /// 不等于操作符 <>
    /// </summary>
    NotEqual,

    /// <summary>
    /// 大于操作符 >
    /// </summary>
    GreaterThan,

    /// <summary>
    /// 小于操作符 <
    /// </summary>
    LessThan,

    /// <summary>
    /// 大于等于操作符 >=
    /// </summary>
    GreaterThanEqual,

    /// <summary>
    /// 小于等于操作符 <=
    /// </summary>
    LessThanEqual,

    /// <summary>
    /// LIKE 操作符
    /// </summary>
    Like,

    /// <summary>
    /// NOT LIKE 操作符
    /// </summary>
    NotLike,

    /// <summary>
    /// BETWEEN 操作符
    /// </summary>
    Between,

    /// <summary>
    /// IN 操作符
    /// </summary>
    In,

    /// <summary>
    /// NOT IN 操作符
    /// </summary>
    NotIn,

    /// <summary>
    /// AND 操作符
    /// </summary>
    And
}
