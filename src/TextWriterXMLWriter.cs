using FurinaXML.Nodes.Ref;
using System.Buffers;
using System.Text;

namespace FurinaXML;

public class TextWriterXMLWriter(TextWriter writer, bool leaveOpen = false) : AbstractXMLWriter, IDisposable
{
    protected override void Write(scoped ReadOnlySpan<byte> utf8data)
    {
        Span<char> buffer = stackalloc char[2];
        while (!utf8data.IsEmpty)
        {
            if (Rune.DecodeFromUtf8(utf8data, out Rune rune, out int consumed) is not OperationStatus.Done)
                throw new InvalidDataException();
            utf8data = utf8data[consumed..];
            int i = rune.EncodeToUtf16(buffer);
            writer.Write(buffer[..i]);
        }
    }
    protected override void Write(scoped ReadOnlySpan<char> utf16data)
        => writer.Write(utf16data);
    protected override void Write(char ch)
        => writer.Write(ch);


    public override void WriteAttribute(scoped RefNameValuePair<byte> attribute)
    {
        if (LastToken is not (TokenType.STagStart or TokenType.Attribute))
            throw new InvalidOperationException();
        Write(' ');
        Write(attribute.Name);
        Write("=\"");
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
                    Write("&lt;");
                    break;
                case '&':
                    Write("&amp;");
                    break;
                case '"':
                    Write("&quot;");
                    break;
                default:
                    if (Rune.IsControl(rune))
                    {
                        Write("&#x");
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
    public override void WriteAttribute(scoped RefNameValuePair<char> attribute)
    {
        if (LastToken is not (TokenType.STagStart or TokenType.Attribute))
            throw new InvalidOperationException();
        Write(' ');
        Write(attribute.Name);
        Write("=\"");
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
                    Write("&lt;");
                    break;
                case '&':
                    Write("&amp;");
                    break;
                case '"':
                    Write("&quot;");
                    break;
                default:
                    if (Rune.IsControl(rune))
                    {
                        Write("&#x");
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

    public override void WriteComment(scoped ReadOnlySpan<byte> content)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!--");
        Write(content);
        Write("-->");
        LastToken = TokenType.Comment;
    }
    public override void WriteComment(scoped ReadOnlySpan<char> content)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!--");
        Write(content);
        Write("-->");
        LastToken = TokenType.Comment;
    }

    public override void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<byte> decl)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!DOCTYPE ");
        Write(decl.Name);
        if (decl.HasExternalID)
        {
            if (decl.IsExternalIDPublic)
            {
                Write(" PUBLIC \"");
                Write(decl.PubIDLiteral);
                Write('"');
            }
            else
            {
                Write(" SYSTEM");
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
    public override void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<char> decl)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!DOCTYPE ");
        Write(decl.Name);
        if (decl.HasExternalID)
        {
            if (decl.IsExternalIDPublic)
            {
                Write(" PUBLIC \"");
                Write(decl.PubIDLiteral);
                Write('"');
            }
            else
            {
                Write(" SYSTEM");
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

    public override void WriteETag(scoped ReadOnlySpan<byte> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("</");
        Write(name);
        Write('>');
        LastToken = TokenType.ETag;
        Depth--;
    }
    public override void WriteETag(scoped ReadOnlySpan<char> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("</");
        Write(name);
        Write('>');
        LastToken = TokenType.ETag;
        Depth--;
    }

    public override void WriteProcessingInstruction(scoped RefNameValuePair<byte> pi)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<?");
        Write(pi.Name);
        if (!pi.Value.IsEmpty)
        {
            Write(' ');
            Write(pi.Value);
        }
        Write("?>");
        LastToken = TokenType.ProcessingInstruction;
    }
    public override void WriteProcessingInstruction(scoped RefNameValuePair<char> pi)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<?");
        Write(pi.Name);
        if (!pi.Value.IsEmpty)
        {
            Write(' ');
            Write(pi.Value);
        }
        Write("?>");
        LastToken = TokenType.ProcessingInstruction;
    }

    public override void WriteText(scoped ReadOnlySpan<byte> content, bool asCDATA = false)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        if (asCDATA)
        {
            Write("<![CDATA[");
            Write(content);
            Write("]]>");
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
                        Write("&lt;");
                        break;
                    case '&':
                        Write("&amp;");
                        break;
                    case '"':
                        Write("&quot;");
                        break;
                    default:
                        if (Rune.IsControl(rune))
                        {
                            Write("&#x");
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
    public override void WriteText(scoped ReadOnlySpan<char> content, bool asCDATA = false)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        if (asCDATA)
        {
            Write("<![CDATA[");
            Write(content);
            Write("]]>");
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
                        Write("&lt;");
                        break;
                    case '&':
                        Write("&amp;");
                        break;
                    case '"':
                        Write("&quot;");
                        break;
                    default:
                        if (Rune.IsControl(rune))
                        {
                            Write("&#x");
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

    public override void WriteXMLDecl(scoped RefXMLDecl<byte> decl)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Span<byte> buffer = stackalloc byte[10];
        Write("<?xml version=\"1.");
        decl.Version.Minor.TryFormat(buffer, out int consumed);
        Write(buffer[..consumed]);
        Write('"');
        if (!decl.Encoding.IsEmpty)
        {
            Write(" encoding=\"");
            Write(decl.Encoding);
            Write('"');
        }
        switch (decl.Standalone)
        {
            case true:
                Write(" standalone=\"yes\"");
                break;
            case false:
                Write(" standalone=\"no\"");
                break;
        }
        Write("?>");
        LastToken = TokenType.XMLDecl;
    }
    public override void WriteXMLDecl(scoped RefXMLDecl<char> decl)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Span<byte> buffer = stackalloc byte[10];
        Write("<?xml version=\"1.");
        decl.Version.Minor.TryFormat(buffer, out int consumed);
        Write(buffer[..consumed]);
        Write('"');
        if (!decl.Encoding.IsEmpty)
        {
            Write(" encoding=\"");
            Write(decl.Encoding);
            Write('"');
        }
        switch (decl.Standalone)
        {
            case true:
                Write(" standalone=\"yes\"");
                break;
            case false:
                Write(" standalone=\"no\"");
                break;
        }
        Write("?>");
        LastToken = TokenType.XMLDecl;
    }
    public override void Flush()
    {
        writer.Flush();
    }

    public void Dispose()
    {
        if (!leaveOpen)
            writer.Dispose();
        GC.SuppressFinalize(this);
    }
}