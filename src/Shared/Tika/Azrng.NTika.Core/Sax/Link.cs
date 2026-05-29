namespace Azrng.NTika.Core.Sax
{
    public class Link
    {
        public string Uri { get; }
        public string Text { get; }

        public Link(string uri, string text)
        {
            Uri = uri;
            Text = text;
        }

        public override string ToString()
        {
            return $"[{Text}]({Uri})";
        }
    }
}
