namespace Azrng.NTika.Core.Abstractions
{
    public interface IAttributes
    {
        int Length { get; }
        string GetUri(int index);
        string GetLocalName(int index);
        string GetQName(int index);
        string GetType(int index);
        string GetValue(int index);
        int GetIndex(string qName);
        int GetIndex(string uri, string localName);
        string? GetType(string qName);
        string? GetType(string uri, string localName);
        string? GetValue(string qName);
        string? GetValue(string uri, string localName);
    }
}
