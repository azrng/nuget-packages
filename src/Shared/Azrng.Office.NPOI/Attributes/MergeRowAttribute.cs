namespace Azrng.Office.NPOI.Attributes
{
    /// <summary>
    /// 行合并，根据ParentKey合并
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class MergeRowAttribute : Attribute
    {
    }
}
