namespace Azrng.Office.NPOI.Attributes
{
    /// <summary>
    /// 单独合并行,优先级大于Rowmerged,与主键无关,只与当前值有关
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class MergeRowAloneAttribute : Attribute
    {
    }
}
