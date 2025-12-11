namespace Azrng.Office.NPOI.Attributes
{
    /// <summary>
    /// 不导出列
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class IgnoreColumnAttribute : Attribute { }
}