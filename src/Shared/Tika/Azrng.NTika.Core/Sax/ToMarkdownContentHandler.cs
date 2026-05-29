using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public class ToMarkdownContentHandler : IContentHandler
    {
        private readonly TextWriter _writer;
        private readonly StringBuilder? _buffer;
        private readonly Stack<string> _elementStack = new();
        private bool _needsNewline;
        private bool _inHeading;
        private int _headingLevel;
        private bool _inCodeBlock;
        private int _listDepth;
        private readonly Stack<bool> _orderedListStack = new();
        private string? _linkHref;
        private int _suppressOutputDepth;

        public ToMarkdownContentHandler(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public ToMarkdownContentHandler()
        {
            _buffer = new StringBuilder();
            _writer = new StringWriter(_buffer);
        }

        public void SetDocumentLocator(ILocator locator) { }
        public void StartDocument() { }

        public void EndDocument()
        {
            _writer.Flush();
        }

        public void StartPrefixMapping(string prefix, string uri) { }
        public void EndPrefixMapping(string prefix) { }

        public void StartElement(string uri, string localName, string qName, IAttributes atts)
        {
            var ln = localName.ToLowerInvariant();
            _elementStack.Push(ln);

            if (ln == "head" || ln == "script" || ln == "style")
            {
                _suppressOutputDepth++;
                return;
            }

            if (_suppressOutputDepth > 0)
                return;

            switch (ln)
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    EnsureNewline();
                    _inHeading = true;
                    _headingLevel = ln[1] - '0';
                    _writer.Write(new string('#', _headingLevel));
                    _writer.Write(' ');
                    break;

                case "p":
                case "div":
                    EnsureNewline();
                    break;

                case "br":
                    _writer.Write("  \n");
                    break;

                case "hr":
                    EnsureNewline();
                    _writer.Write("---\n");
                    break;

                case "ul":
                    _listDepth++;
                    _orderedListStack.Push(false);
                    EnsureNewline();
                    break;

                case "ol":
                    _listDepth++;
                    _orderedListStack.Push(true);
                    EnsureNewline();
                    break;

                case "li":
                    WriteIndent();
                    if (_orderedListStack.Count > 0 && _orderedListStack.Peek())
                        _writer.Write("1. ");
                    else
                        _writer.Write("- ");
                    break;

                case "table":
                    EnsureNewline();
                    break;

                case "tr":
                    _writer.Write("| ");
                    break;

                case "th":
                case "td":
                    break;

                case "a":
                    _linkHref = atts.GetValue("href");
                    _writer.Write('[');
                    break;

                case "b":
                case "strong":
                    _writer.Write("**");
                    break;

                case "i":
                case "em":
                    _writer.Write('*');
                    break;

                case "code":
                    if (!_inCodeBlock)
                        _writer.Write('`');
                    break;

                case "pre":
                    EnsureNewline();
                    _writer.Write("```\n");
                    _inCodeBlock = true;
                    break;

                case "blockquote":
                    EnsureNewline();
                    _writer.Write("> ");
                    break;

            }
        }

        public void EndElement(string uri, string localName, string qName)
        {
            if (_elementStack.Count > 0)
                _elementStack.Pop();

            var ln = localName.ToLowerInvariant();

            if (_suppressOutputDepth > 0)
            {
                if (ln == "head" || ln == "script" || ln == "style")
                    _suppressOutputDepth--;
                return;
            }

            switch (ln)
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    _writer.Write('\n');
                    _inHeading = false;
                    _needsNewline = true;
                    break;

                case "p":
                    _writer.Write("\n\n");
                    _needsNewline = false;
                    break;

                case "div":
                    _writer.Write('\n');
                    _needsNewline = true;
                    break;

                case "ul":
                case "ol":
                    if (_orderedListStack.Count > 0)
                        _orderedListStack.Pop();
                    _listDepth--;
                    if (_listDepth == 0)
                        _writer.Write('\n');
                    break;

                case "li":
                    _writer.Write('\n');
                    break;

                case "table":
                    _writer.Write('\n');
                    break;

                case "tr":
                    _writer.Write(" |\n");
                    break;

                case "th":
                    _writer.Write(" | ");
                    break;

                case "td":
                    _writer.Write(" | ");
                    break;

                case "a":
                    _writer.Write("](");
                    _writer.Write(EscapeLinkDestination(_linkHref));
                    _writer.Write(')');
                    _linkHref = null;
                    break;

                case "b":
                case "strong":
                    _writer.Write("**");
                    break;

                case "i":
                case "em":
                    _writer.Write('*');
                    break;

                case "code":
                    if (!_inCodeBlock)
                        _writer.Write('`');
                    break;

                case "pre":
                    _writer.Write("\n```\n");
                    _inCodeBlock = false;
                    break;
            }
        }

        public void Characters(char[] ch, int start, int length)
        {
            if (_suppressOutputDepth > 0)
                return;

            var text = new string(ch, start, length);

            if (_inCodeBlock)
            {
                _writer.Write(text);
                return;
            }

            if (_inHeading)
            {
                _writer.Write(text.Trim());
                return;
            }

            _writer.Write(text);
        }

        public void IgnorableWhitespace(char[] ch, int start, int length)
        {
            Characters(ch, start, length);
        }

        public void ProcessingInstruction(string target, string data) { }
        public void SkippedEntity(string name) { }

        public override string ToString()
        {
            if (_buffer != null)
            {
                _writer.Flush();
                return _buffer.ToString();
            }
            return string.Empty;
        }

        private void EnsureNewline()
        {
            if (_needsNewline)
            {
                _writer.Write('\n');
                _needsNewline = false;
            }
        }

        private void WriteIndent()
        {
            for (int i = 1; i < _listDepth; i++)
                _writer.Write("  ");
        }

        private static string EscapeLinkDestination(string? href)
        {
            if (string.IsNullOrEmpty(href))
                return string.Empty;

            return href
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("[", "\\[")
                .Replace("]", "\\]");
        }
    }
}
