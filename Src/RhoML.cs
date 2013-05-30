﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util.Collections;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace RT.Util
{
    /// <summary>Exposes methods related to the RhoML language.</summary>
    public static class RhoML
    {
        /// <summary>
        ///     Parses the specified string as RhoML.</summary>
        /// <param name="input">
        ///     The string to parse.</param>
        /// <returns>
        ///     The root element of the parse tree. This element is a tag with no name, even if the entire input was a text
        ///     node.</returns>
        public static RhoTag Parse(string input)
        {
            return new RhoParserState(input).Parse();
        }
    }

    /// <summary>Encapsulates one of the two possible types of RhoML nodes.</summary>
    public abstract class RhoNode
    {
        /// <summary>
        ///     Appends this node and all children to the specified string builder, converting them to RhoML format that would
        ///     parse back into this tree.</summary>
        public abstract void AppendTo(StringBuilder builder);

        /// <summary>Converts this node to a RhoML string.</summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            AppendTo(sb);
            return sb.ToString();
        }
    }

    /// <summary>Encapsulates a text node in a RhoML tree.</summary>
    public sealed class RhoText : RhoNode
    {
        /// <summary>Gets or sets the text string represented by this instance. Not null.</summary>
        public string Text
        {
            get { return _text; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _text = value;
            }
        }

        private string _text;

        /// <summary>Constructor.</summary>
        public RhoText(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            Text = text;
        }

        /// <summary>
        ///     Appends this node and all children to the specified string builder, converting them to RhoML format that would
        ///     parse back into this tree.</summary>
        public override void AppendTo(StringBuilder builder)
        {
            builder.Append(Regex.Replace(_text, @"{[`}a-zA-Z]", match => "{" + match.Value));
        }
    }

    /// <summary>Encapsulates a tag node in a RhoML tree.</summary>
    public sealed class RhoTag : RhoNode
    {
        /// <summary>Gets or sets the name of the tag. Null for the root tag, otherwise non-null.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the value of the default attribute. Null if none, empty string if has an empty value.</summary>
        public string Value { get; set; }
        /// <summary>Gets or sets a dictionary of attributes. Not null. Value is null if none, empty string if has an empty value.</summary>
        public IDictionary<string, string> Attributes
        {
            get { return _attributes; }
            set { if (value == null) throw new ArgumentNullException("value"); _attributes = value; }
        }
        /// <summary>
        ///     Gets or sets a read-only list of child elements. Not null. May be empty, or contain any <see cref="RhoNode"/>
        ///     instance in any order whatsoever.</summary>
        public IList<RhoNode> Children
        {
            get { return _children; }
            set { if (value == null) throw new ArgumentNullException("value"); _children = value; }
        }

        private IDictionary<string, string> _attributes;
        private IList<RhoNode> _children;

        /// <summary>Constructor.</summary>
        public RhoTag()
        {
            _attributes = new Dictionary<string, string>();
            _children = new List<RhoNode>();
        }

        /// <summary>Constructor.</summary>
        public RhoTag(string name, string value = null)
            : this()
        {
            Name = name;
            Value = value;
        }

        /// <summary>Constructor.</summary>
        public RhoTag(string name, string value, IDictionary<string, string> attributes, List<RhoNode> elements)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");
            if (elements == null)
                throw new ArgumentNullException("elements");
            Name = name;
            Value = value;
            Attributes = new ReadOnlyDictionary<string, string>(attributes);
            Children = elements.AsReadOnly();
        }

        /// <summary>Enumerates all descendants of this node, in an unspecified order.</summary>
        public IEnumerable<RhoNode> Descendants
        {
            get
            {
                var queue = new Queue<RhoNode>();
                queue.EnqueueRange(this.Children);

                while (queue.Any())
                {
                    var cur = queue.Dequeue();
                    yield return cur;
                    var tag = cur as RhoTag;
                    if (tag != null)
                        queue.EnqueueRange(tag.Children);
                }
            }
        }

        /// <summary>
        ///     Appends this node and all children to the specified string builder, converting them to RhoML format that would
        ///     parse back into this tree.</summary>
        public override void AppendTo(StringBuilder builder)
        {
            if (Name != null)
            {
                builder.Append('{');
                builder.Append(escapedAttrString(Name));
                if (Value != null)
                {
                    builder.Append('=');
                    builder.Append(escapedAttrString(Value));
                }
                foreach (var attr in Attributes)
                {
                    builder.Append(',');
                    builder.Append(escapedAttrString(attr.Key));
                    if (attr.Value != null)
                    {
                        builder.Append('=');
                        builder.Append(escapedAttrString(attr.Value));
                    }
                }
                builder.Append('}');
            }

            foreach (var element in Children)
                element.AppendTo(builder);

            if (Name != null)
                builder.Append("{}");
        }

        private string escapedAttrString(string str)
        {
            if (str.Length == 0)
                return "";
            if (str[0] == ' ' || str[str.Length - 1] == ' ' || str.Any(c => c == '}' || c == '=' || c == ',' || c == '`' || c == '\r' || c == '\n' || c == '\t'))
                return "`" + str.Replace("`", "``") + "`";
            else
                return str;
        }
    }

    internal sealed class RhoParserState
    {
        public string Input;
        public int Pos;

        private OffsetToLineCol _offsetConverter;
        public OffsetToLineCol OffsetConverter { get { if (_offsetConverter == null) _offsetConverter = new OffsetToLineCol(Input); return _offsetConverter; } }

        private RhoParserState() { }

        public RhoParserState(string input)
        {
            Input = input;
            Pos = 0;
        }

        public RhoParserState Clone()
        {
            var result = new RhoParserState();
            result.Input = Input;
            result.Pos = Pos;
            result._offsetConverter = _offsetConverter;
            return result;
        }

        public char? Cur { get { return Pos >= Input.Length ? null : (char?) Input[Pos]; } }
        public char? Next { get { return Pos + 1 >= Input.Length ? null : (char?) Input[Pos + 1]; } }

        public string Snippet
        {
            get
            {
                int line, col;
                OffsetConverter.GetLineAndColumn(Pos, out line, out col);
                return "Before: {2}   After: {3}   At: {0},{1}".Fmt(line, col, Input.SubstringSafe(Pos - 15, 15), Input.SubstringSafe(Pos, 15));
            }
        }

        public override string ToString()
        {
            return Snippet;
        }

        public RhoTag Parse()
        {
            var elems = parseElements();
            if (Cur != null)
                throw new RhoParseException(this, "Expected end of input");
            return new RhoTag(null, null, new Dictionary<string, string>(), elems);
        }

        private List<RhoNode> parseElements()
        {
            var result = new List<RhoNode>();
            while (true)
            {
                string consumed = consumeUntilNonText();
                if (consumed.Length > 0)
                    result.Add(new RhoText(consumed));
                if (Pos >= Input.Length)
                    return result;
                if (Cur == '{' && Next == '}')
                    return result;
                result.Add(parseTag());
            }
        }

        private void consumeWhitespace()
        {
            while (Pos < Input.Length && " \t\r\n".Contains(Input[Pos]))
                Pos++;
        }

        private string consumeUntilNonText()
        {
            int pos = Pos;

            var sb = new StringBuilder();
            while (pos < Input.Length)
            {
                // Last char before EOF can only be text
                if (pos + 1 >= Input.Length)
                {
                    pos++;
                    break;
                }

                if (Input[pos] == '{')
                {
                    char next = Input[pos + 1];
                    if (next == '`' || next == '}' || (next >= 'a' && next <= 'z') || (next >= 'A' && next <= 'Z'))
                        break;
                    // Otherwise the next character cannot possibly start a tag
                    if (next == '{')
                    {
                        sb.Append(Input.Substring(Pos, pos - Pos));
                        Pos = pos + 1;
                    }
                    pos++;
                }

                pos++;
            }
            sb.Append(Input.Substring(Pos, pos - Pos));
            Pos = pos;
            return sb.ToString();
        }

        private RhoTag parseTag()
        {
            // Opening tag
            if (Cur != '{')
                throw new RhoParseException(this, "Expected '{'");
            Pos++;
            string name = parseAttrString();

            string value = null;
            if (Cur == '=')
            {
                Pos++;
                consumeWhitespace();
                value = parseAttrString();
            }

            var attrs = new Dictionary<string, string>();
            while (Cur == ',')
            {
                Pos++;
                consumeWhitespace();
                string attrName = parseAttrString();
                string attrValue = null;
                if (Cur == '=')
                {
                    Pos++;
                    consumeWhitespace();
                    attrValue = parseAttrString();
                }
                attrs[attrName] = attrValue;
            }

            if (Cur != '}')
                throw new RhoParseException(this, "Expected '}' (or ',' or possibly '=')");
            Pos++;

            // Content
            var elems = parseElements();

            // Closing tag
            if (Cur != '{' || Next != '}')
                throw new RhoParseException(this, "Expected '{}' - the closing tag");
            Pos += 2;

            // Done!
            return new RhoTag(name, value, attrs, elems);
        }

        private string parseAttrString()
        {
            if (Cur == '`')
                return parseAttrStringQuoted();
            else
                return parseAttrStringRaw();
        }

        private string parseAttrStringQuoted()
        {
            if (Cur != '`')
                throw new RhoParseException(this, "Expected '`'");
            Pos++;
            var result = new StringBuilder();
            int pos = Pos;
            while (pos < Input.Length)
            {
                char c = Input[pos];
                if (c == '`')
                {
                    if (pos + 1 < Input.Length && Input[pos + 1] == '`')
                    {
                        // this ` is escaped
                        result.Append(Input.Substring(Pos, pos - Pos + 1));
                        pos++;
                        Pos = pos + 1;
                    }
                    else
                        break; // not escaped: definitely a string-closing `
                }
                pos++;
            }
            result.Append(Input.Substring(Pos, pos - Pos));
            Pos = pos;
            if (Cur != '`')
                throw new RhoParseException(this, "Expected '`'");
            Pos++;
            consumeWhitespace();
            return result.ToString(); ;
        }

        private string parseAttrStringRaw()
        {
            int pos = Pos;
            while (pos < Input.Length)
            {
                char c = Input[pos];
                if (c == '{' || c == '}' || c == '=' || c == ',' || c == '`' || c == '\r' || c == '\n' || c == '\t')
                    break;
                pos++;
            }
            string result = Input.Substring(Pos, pos - Pos);
            Pos = pos;
            consumeWhitespace();
            return result.Trim(' ');
        }
    }

    /// <summary>Represents a RhoML parsing exception.</summary>
    public class RhoParseException : Exception
    {
        private RhoParserState _state;

        internal RhoParseException(RhoParserState ps, string message)
            : base(message)
        {
            _state = ps.Clone();
        }

        /// <summary>Gets the line number at which the parse error occurred.</summary>
        public int Line { get { return _state.OffsetConverter.GetLine(_state.Pos); } }
        /// <summary>Gets the column number at which the parse error occurred.</summary>
        public int Column { get { return _state.OffsetConverter.GetColumn(_state.Pos); } }
        /// <summary>A snippet of the JSON string at which the parse error occurred.</summary>
        public string Snippet { get { return _state.Snippet; } }
    }

}
