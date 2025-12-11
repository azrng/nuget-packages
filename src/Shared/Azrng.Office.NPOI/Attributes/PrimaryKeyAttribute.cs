namespace Azrng.Office.NPOI.Attributes
{
    /// <summary>
    /// 主键,合并根据该主键进行
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute
    {
    }
}