namespace Azrng.NTika.Core.Abstractions
{
    public interface ILocator
    {
        string? PublicId { get; }
        string? SystemId { get; }
        int LineNumber { get; }
        int ColumnNumber { get; }
    }
}
