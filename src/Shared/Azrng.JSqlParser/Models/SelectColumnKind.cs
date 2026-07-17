namespace Azrng.JSqlParser.Models;

/// <summary>
/// SELECT 列项的分类。
/// </summary>
public enum SelectColumnKind
{
    /// <summary>裸 <c>*</c>（SELECT *）。</summary>
    All,

    /// <summary>限定表的全列（如 <c>t.*</c>）。</summary>
    AllTable,

    /// <summary>普通列引用（如 <c>u.name</c>）。</summary>
    Column,

    /// <summary>表达式列（函数、算术、CASE 等非裸列引用）。</summary>
    Expression
}
