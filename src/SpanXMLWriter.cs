using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;
using System.Buffers;
using System.Text;

namespace FurinaXML;

public ref struct SpanXMLWriter : IXMLWriter
{
    private Span<byte> dataB;
    private Span<char> dataC;
    public readonly bool IsTargetUTF8;
    public TokenType LastToken { get; private set; } = TokenType.None;
    public int Depth { get; private set; } = 0;

    public SpanXMLWriter(Span<byte> data)
    {
        dataB = data;
        IsTargetUTF8 = true;
    }
    public SpanXMLWriter(Span<char> data)
    {
        dataC = data;
        IsTargetUTF8 = false;
    }

    private void Write(scoped ReadOnlySpan<byte> utf8data)
    {
        if (IsTargetUTF8)
        {
            utf8data.CopyTo(dataB);
            dataB = dataB[utf8data.Length..];
        }
        else
        {
            while (!utf8data.IsEmpty)
            {
                if (Rune.DecodeFromUtf8(utf8data, out Rune rune, out int consumed) is not OperationStatus.Done)
                    throw new InvalidDataException();
                utf8data = utf8data[consumed..];
                consumed = rune.EncodeToUtf16(dataC);
                dataC = dataC[consumed..];
            }
        }
    }
    private void Write(scoped ReadOnlySpan<char> utf16data)
    {
        if (IsTargetUTF8)
        {
            while (!utf16data.IsEmpty)
            {
                if (Rune.DecodeFromUtf16(utf16data, out Rune rune, out int consumed) is not OperationStatus.Done)
                    throw new InvalidDataException();
                utf16data = utf16data[consumed..];
                consumed = rune.EncodeToUtf8(dataB);
                dataB = dataB[consumed..];
            }
        }
        else
        {
            utf16data.CopyTo(dataC);
            dataC = dataC[utf16data.Length..];
        }
    }
    private void Write(char ch)
    {
        if (IsTargetUTF8)
        {
            Rune rune = new(ch);
            int i = rune.EncodeToUtf8(dataB);
            dataB = dataB[i..];
        }
        else
        {
            dataC[0] = ch;
            dataC = dataC[1..];
        }
    }

    public void WriteAttribute(scoped RefNameValuePair<byte> attribute)
    {
        if (LastToken is not (TokenType.STagStart or TokenType.Attribute))
            throw new InvalidOperationException();
        Write(' ');
        Write(attribute.Name);
        Write("=\""u8);
        ReadOnlySpan<byte> attrValue = attribute.Value;
        Span<byte> buffer = stackalloc byte[6];
        while (!attrValue.IsEmpty)
        {
            if (Rune.DecodeFromUtf8(attrValue, out Rune rune, out int consumed) is not OperationStatus.Done)
                throw new ArgumentException("Invalid code point", nameof(attribute));

            attrValue = attrValue[consumed..];
            switch (rune.Value)
            {
                case '<':
                    Write("&lt;"u8);
                    break;
                case '&':
                    Write("&amp;"u8);
                    break;
                case '"':
                    Write("&quot;"u8);
                    break;
                default:
                    if (Rune.IsControl(rune))
                    {
                        Write("&#x"u8);
                        rune.Value.TryFormat(buffer, out consumed);
                        Write(buffer[..consumed]);
                        Write(';');
                    }
                    else
                    {
                        consumed = rune.EncodeToUtf8(buffer);
                        Write(buffer[..consumed]);
                    }
                    break;
            }
        }
        Write('"');
        LastToken = TokenType.Attribute;
    }
    public void WriteAttribute(scoped RefNameValuePair<char> attribute)
    {
        if (LastToken is not (TokenType.STagStart or TokenType.Attribute))
            throw new InvalidOperationException();
        Write(' ');
        Write(attribute.Name);
        Write("=\""u8);
        ReadOnlySpan<char> attrValue = attribute.Value;
        Span<byte> buffer = stackalloc byte[6];
        while (!attrValue.IsEmpty)
        {
            if (Rune.DecodeFromUtf16(attrValue, out Rune rune, out int consumed) is not OperationStatus.Done)
                throw new ArgumentException("Invalid code point", nameof(attribute));

            attrValue = attrValue[consumed..];
            switch (rune.Value)
            {
                case '<':
                    Write("&lt;"u8);
                    break;
                case '&':
                    Write("&amp;"u8);
                    break;
                case '"':
                    Write("&quot;"u8);
                    break;
                default:
                    if (Rune.IsControl(rune))
                    {
                        Write("&#x"u8);
                        rune.Value.TryFormat(buffer, out consumed);
                        Write(buffer[..consumed]);
                        Write(';');
                    }
                    else
                    {
                        consumed = rune.EncodeToUtf8(buffer);
                        Write(buffer[..consumed]);
                    }
                    break;
            }
        }
        Write('"');
        LastToken = TokenType.Attribute;
    }
    public void WriteAttribute(XMLAttribute attribute)
    {
        WriteAttribute(new RefNameValuePair<char>
        {
            Name = attribute.Name,
            Value = attribute.Value
        });
    }

    public void WriteAttributes(params IEnumerable<XMLAttribute>? attributes)
    {
        if (attributes is null)
            return;
        foreach (XMLAttribute attribute in attributes)
            WriteAttribute(attribute);
    }
    public void WriteAttributes(params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        foreach (XMLAttribute attribute in attributes)
            WriteAttribute(attribute);
    }

    public void WriteComment(scoped ReadOnlySpan<byte> content)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!--"u8);
        Write(content);
        Write("-->"u8);
        LastToken = TokenType.Comment;
    }
    public void WriteComment(scoped ReadOnlySpan<char> content)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!--"u8);
        Write(content);
        Write("-->"u8);
        LastToken = TokenType.Comment;
    }
    public void WriteComment(XMLComment content)
    {
        WriteComment(content.InnerText);
    }

    public void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<byte> decl)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!DOCTYPE "u8);
        Write(decl.Name);
        if (decl.HasExternalID)
        {
            if (decl.IsExternalIDPublic)
            {
                Write(" PUBLIC \""u8);
                Write(decl.PubIDLiteral);
                Write('"');
            }
            else
            {
                Write(" SYSTEM"u8);
            }
            Write(' ');
            if (decl.SystemLiteral.Contains((byte)'"'))
            {
                Write('\"');
                Write(decl.SystemLiteral);
                Write('\"');
            }
            else
            {
                Write('"');
                Write(decl.SystemLiteral);
                Write('"');
            }
        }
        Write('>');
        LastToken = TokenType.DocTypeDecl;
    }
    public void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<char> decl)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!DOCTYPE "u8);
        Write(decl.Name);
        if (decl.HasExternalID)
        {
            if (decl.IsExternalIDPublic)
            {
                Write(" PUBLIC \""u8);
                Write(decl.PubIDLiteral);
                Write('"');
            }
            else
            {
                Write(" SYSTEM"u8);
            }
            Write(' ');
            if (decl.SystemLiteral.Contains('"'))
            {
                Write('\"');
                Write(decl.SystemLiteral);
                Write('\"');
            }
            else
            {
                Write('"');
                Write(decl.SystemLiteral);
                Write('"');
            }
        }
        Write('>');
        LastToken = TokenType.DocTypeDecl;
    }
    public void WriteDocTypeDecl(XMLDocTypeDecl decl)
    {
        WriteDocTypeDecl(new RefXMLDocTypeDecl<char>
        {
            Name = decl.Name,
            HasExternalID = decl.HasExternalID,
            IsExternalIDPublic = decl.IsExternalIDPublic,
            PubIDLiteral = decl.PubIDLiteral,
            SystemLiteral = decl.SystemLiteral,
        });
    }

    public void WriteElement(XMLElement element)
    {
        WriteSTag(element.Name, element.Attributes);
        foreach (IXMLNode node in element.Children)
            WriteNode(node);
        WriteETag(element.Name);
    }

    public void WriteEmptyTag(scoped ReadOnlySpan<byte> name, params IEnumerable<XMLAttribute>? attributes)
    {
        WriteSTag(name);
        WriteAttributes(attributes);
        WriteSTagEnd(true);
    }
    public void WriteEmptyTag(scoped ReadOnlySpan<char> name, params IEnumerable<XMLAttribute>? attributes)
    {
        WriteSTag(name);
        WriteAttributes(attributes);
        WriteSTagEnd(true);
    }
    public void WriteEmptyTag(scoped ReadOnlySpan<byte> name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        WriteSTag(name);
        WriteAttributes(attributes);
        WriteSTagEnd(true);
    }
    public void WriteEmptyTag(scoped ReadOnlySpan<char> name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        WriteSTag(name);
        WriteAttributes(attributes);
        WriteSTagEnd(true);
    }

    public void WriteEmptyTag(XMLElement element)
    {
        WriteEmptyTag(element.Name, element.Attributes);
    }

    public void WriteETag(scoped ReadOnlySpan<byte> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("</"u8);
        Write(name);
        Write('>');
        LastToken = TokenType.ETag;
        Depth--;
    }
    public void WriteETag(scoped ReadOnlySpan<char> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("</"u8);
        Write(name);
        Write('>');
        LastToken = TokenType.ETag;
        Depth--;
    }

    public void WriteNode(IXMLNode node)
    {
        switch (node)
        {
            case XMLElement element:
                WriteElement(element);
                break;
            case XMLTextNode text:
                WriteText(text);
                break;
            case XMLComment comment:
                WriteComment(comment);
                break;
            case XMLDecl xmlDecl:
                WriteXMLDecl(xmlDecl);
                break;
            case XMLDocTypeDecl doctypeDecl:
                WriteDocTypeDecl(doctypeDecl);
                break;
            case XMLProcessingInstruction pi:
                WriteProcessingInstruction(pi);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public void WriteProcessingInstruction(scoped RefNameValuePair<byte> pi)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<?"u8);
        Write(pi.Name);
        if (!pi.Value.IsEmpty)
        {
            Write(' ');
            Write(pi.Value);
        }
        Write("?>"u8);
        LastToken = TokenType.ProcessingInstruction;
    }
    public void WriteProcessingInstruction(scoped RefNameValuePair<char> pi)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<?"u8);
        Write(pi.Name);
        if (!pi.Value.IsEmpty)
        {
            Write(' ');
            Write(pi.Value);
        }
        Write("?>"u8);
        LastToken = TokenType.ProcessingInstruction;
    }
    public void WriteProcessingInstruction(XMLProcessingInstruction pi)
    {
        WriteAttribute(new RefNameValuePair<char>
        {
            Name = pi.Name,
            Value = pi.Value
        });
    }

    public void WriteSTag(scoped ReadOnlySpan<byte> name, params IEnumerable<XMLAttribute>? attributes)
    {
        WriteSTagStart(name);
        WriteAttributes(attributes);
    }
    public void WriteSTag(scoped ReadOnlySpan<char> name, params IEnumerable<XMLAttribute>? attributes)
    {
        WriteSTagStart(name);
        WriteAttributes(attributes);
    }
    public void WriteSTag(scoped ReadOnlySpan<byte> name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        WriteSTagStart(name);
        WriteAttributes(attributes);
    }
    public void WriteSTag(scoped ReadOnlySpan<char> name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        WriteSTagStart(name);
        WriteAttributes(attributes);
    }

    public void WriteSTagEnd(bool isEmptyTag)
    {
        if (LastToken is not (TokenType.STagStart or TokenType.Attribute))
            throw new InvalidOperationException();
        if (isEmptyTag)
        {
            Write('/');
            LastToken = TokenType.EmptyTag;
            Depth--;
        }
        else
            LastToken = TokenType.STagEnd;
        Write('>');
    }

    public void WriteSTagStart(scoped ReadOnlySpan<byte> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write('<');
        Write(name);
        LastToken = TokenType.STagStart;
        Depth++;
    }
    public void WriteSTagStart(scoped ReadOnlySpan<char> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write('<');
        Write(name);
        LastToken = TokenType.STagStart;
        Depth++;
    }

    public void WriteText(scoped ReadOnlySpan<byte> content, bool asCDATA = false)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        if (asCDATA)
        {
            Write("<![CDATA["u8);
            Write(content);
            Write("]]>"u8);
        }
        else
        {
            Span<byte> buffer = stackalloc byte[6];
            while (!content.IsEmpty)
            {
                if (Rune.DecodeFromUtf8(content, out Rune rune, out int consumed) is not OperationStatus.Done)
                    throw new ArgumentException("Invalid code point", nameof(content));

                content = content[consumed..];
                switch (rune.Value)
                {
                    case '<':
                        Write("&lt;"u8);
                        break;
                    case '&':
                        Write("&amp;"u8);
                        break;
                    case '"':
                        Write("&quot;"u8);
                        break;
                    default:
                        if (Rune.IsControl(rune))
                        {
                            Write("&#x"u8);
                            rune.Value.TryFormat(buffer, out consumed);
                            Write(buffer[..consumed]);
                            Write(';');
                        }
                        else
                        {
                            consumed = rune.EncodeToUtf8(buffer);
                            Write(buffer[..consumed]);
                        }
                        break;
                }
            }
        }
        LastToken = TokenType.Text;
    }
    public void WriteText(scoped ReadOnlySpan<char> content, bool asCDATA = false)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        if (asCDATA)
        {
            Write("<![CDATA["u8);
            Write(content);
            Write("]]>"u8);
        }
        else
        {
            Span<byte> buffer = stackalloc byte[6];
            while (!content.IsEmpty)
            {
                if (Rune.DecodeFromUtf16(content, out Rune rune, out int consumed) is not OperationStatus.Done)
                    throw new ArgumentException("Invalid code point", nameof(content));

                content = content[consumed..];
                switch (rune.Value)
                {
                    case '<':
                        Write("&lt;"u8);
                        break;
                    case '&':
                        Write("&amp;"u8);
                        break;
                    case '"':
                        Write("&quot;"u8);
                        break;
                    default:
                        if (Rune.IsControl(rune))
                        {
                            Write("&#x"u8);
                            rune.Value.TryFormat(buffer, out consumed);
                            Write(buffer[..consumed]);
                            Write(';');
                        }
                        else
                        {
                            consumed = rune.EncodeToUtf8(buffer);
                            Write(buffer[..consumed]);
                        }
                        break;
                }
            }
        }
        LastToken = TokenType.Text;
    }
    public void WriteText(XMLTextNode node, bool asCDATA = false)
    {
        WriteText(node.InnerText, asCDATA);
    }

    public void WriteXMLDecl(scoped RefXMLDecl<byte> decl)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Span<byte> buffer = stackalloc byte[10];
        Write("<?xml version=\"1."u8);
        decl.Version.Minor.TryFormat(buffer, out int consumed);
        Write(buffer[..consumed]);
        Write('"');
        if (!decl.Encoding.IsEmpty)
        {
            Write(" encoding=\""u8);
            Write(decl.Encoding);
            Write('"');
        }
        switch (decl.Standalone)
        {
            case true:
                Write(" standalone=\"yes\""u8);
                break;
            case false:
                Write(" standalone=\"no\""u8);
                break;
        }
        Write("?>"u8);
        LastToken = TokenType.XMLDecl;
    }
    public void WriteXMLDecl(scoped RefXMLDecl<char> decl)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Span<byte> buffer = stackalloc byte[10];
        Write("<?xml version=\"1."u8);
        decl.Version.Minor.TryFormat(buffer, out int consumed);
        Write(buffer[..consumed]);
        Write('"');
        if (!decl.Encoding.IsEmpty)
        {
            Write(" encoding=\""u8);
            Write(decl.Encoding);
            Write('"');
        }
        switch (decl.Standalone)
        {
            case true:
                Write(" standalone=\"yes\""u8);
                break;
            case false:
                Write(" standalone=\"no\""u8);
                break;
        }
        Write("?>"u8);
        LastToken = TokenType.XMLDecl;
    }
    public void WriteXMLDecl(XMLDecl decl)
    {
        WriteXMLDecl(new RefXMLDecl<char>
        {
            Version = decl.Version,
            Encoding = decl.Encoding,
            Standalone = decl.Standalone,
        });
    }

    public void WriteLine()
    {
        Write('\n');
    }
    public void WriteSpace(int count)
    {
        while (count-- > 0)
            Write(' ');
    }
}