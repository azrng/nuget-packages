using System.Collections.Generic;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Sax
{
    public class ContentEventBuffer : IContentHandler
    {
        private readonly List<ContentEvent> _events = new();

        public void SetDocumentLocator(ILocator locator) { }
        public void StartDocument() { }
        public void EndDocument() { }
        public void StartPrefixMapping(string prefix, string uri) { }
        public void EndPrefixMapping(string prefix) { }

        public void StartElement(string uri, string localName, string qName, IAttributes atts)
        {
            var copy = new AttributesImpl();
            for (var i = 0; i < atts.Length; i++)
            {
                copy.AddAttribute(
                    atts.GetUri(i),
                    atts.GetLocalName(i),
                    atts.GetQName(i),
                    atts.GetType(i),
                    atts.GetValue(i));
            }

            _events.Add(new StartElementEvent(uri, localName, qName, copy));
        }

        public void EndElement(string uri, string localName, string qName)
        {
            _events.Add(new EndElementEvent(uri, localName, qName));
        }

        public void Characters(char[] ch, int start, int length)
        {
            _events.Add(new CharactersEvent(new string(ch, start, length)));
        }

        public void IgnorableWhitespace(char[] ch, int start, int length)
        {
            Characters(ch, start, length);
        }

        public void ProcessingInstruction(string target, string data) { }
        public void SkippedEntity(string name) { }

        public void Replay(IContentHandler handler)
        {
            foreach (var contentEvent in _events)
            {
                contentEvent.Replay(handler);
            }
        }

        private abstract class ContentEvent
        {
            public abstract void Replay(IContentHandler handler);
        }

        private sealed class StartElementEvent : ContentEvent
        {
            private readonly string _uri;
            private readonly string _localName;
            private readonly string _qName;
            private readonly IAttributes _attributes;

            public StartElementEvent(string uri, string localName, string qName, IAttributes attributes)
            {
                _uri = uri;
                _localName = localName;
                _qName = qName;
                _attributes = attributes;
            }

            public override void Replay(IContentHandler handler)
            {
                handler.StartElement(_uri, _localName, _qName, _attributes);
            }
        }

        private sealed class EndElementEvent : ContentEvent
        {
            private readonly string _uri;
            private readonly string _localName;
            private readonly string _qName;

            public EndElementEvent(string uri, string localName, string qName)
            {
                _uri = uri;
                _localName = localName;
                _qName = qName;
            }

            public override void Replay(IContentHandler handler)
            {
                handler.EndElement(_uri, _localName, _qName);
            }
        }

        private sealed class CharactersEvent : ContentEvent
        {
            private readonly string _text;

            public CharactersEvent(string text)
            {
                _text = text;
            }

            public override void Replay(IContentHandler handler)
            {
                var chars = _text.ToCharArray();
                handler.Characters(chars, 0, chars.Length);
            }
        }
    }
}
