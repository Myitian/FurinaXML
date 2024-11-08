using FurinaXML.Nodes.Ref;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace FurinaXML;

public class StreamXMLWriter(Stream stream, bool isTargetUTF8 = true, bool leaveOpen = false) : AbstractXMLWriter, IDisposable
{
    public bool IsTargetUTF8 { get; } = isTargetUTF8;

    protected override void Write(scoped ReadOnlySpan<byte> utf8data)
    {
        if (IsTargetUTF8)
            stream.Write(utf8data);
        else
        {
            Span<char> buffer = stackalloc char[2];
            while (!utf8data.IsEmpty)
            {
                if (Rune.DecodeFromUtf8(utf8data, out Rune rune, out int consumed) is not OperationStatus.Done)
                    throw new InvalidDataException();
                utf8data = utf8data[consumed..];
                int i = rune.EncodeToUtf16(buffer);
                stream.Write(MemoryMarshal.Cast<char, byte>(buffer[..i]));
            }
        }
    }
    protected override void Write(scoped ReadOnlySpan<char> utf16data)
    {
        if (IsTargetUTF8)
        {
            Span<byte> buffer = stackalloc byte[4];
            while (!utf16data.IsEmpty)
            {
                if (Rune.DecodeFromUtf16(utf16data, out Rune rune, out int consumed) is not OperationStatus.Done)
                    throw new InvalidDataException();
                utf16data = utf16data[consumed..];
                consumed = rune.EncodeToUtf8(buffer);
                stream.Write(buffer[..consumed]);
            }
        }
        else
            stream.Write(MemoryMarshal.Cast<char, byte>(utf16data));
    }
    protected override void Write(char ch)
    {
        if (IsTargetUTF8)
        {
            if (ch < 0x80)
                stream.WriteByte((byte)ch);
            else
            {
                Rune rune = new(ch);
                Span<byte> buffer = stackalloc byte[4];
                int i = rune.EncodeToUtf8(buffer);
                stream.Write(buffer[..i]);
            }
        }
        else
        {
            stream.Write(MemoryMarshal.Cast<char, byte>(MemoryMarshal.CreateReadOnlySpan(ref ch, 1)));
        }
    }

    public override void WriteAttribute(scoped RefNameValuePair<byte> attribute)
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
    public override void WriteAttribute(scoped RefNameValuePair<char> attribute)
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

    public override void WriteComment(scoped ReadOnlySpan<byte> content)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!--"u8);
        Write(content);
        Write("-->"u8);
        LastToken = TokenType.Comment;
    }
    public override void WriteComment(scoped ReadOnlySpan<char> content)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("<!--"u8);
        Write(content);
        Write("-->"u8);
        LastToken = TokenType.Comment;
    }

    public override void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<byte> decl)
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
    public override void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<char> decl)
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

    public override void WriteETag(scoped ReadOnlySpan<byte> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("</"u8);
        Write(name);
        Write('>');
        LastToken = TokenType.ETag;
        Depth--;
    }
    public override void WriteETag(scoped ReadOnlySpan<char> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write("</"u8);
        Write(name);
        Write('>');
        LastToken = TokenType.ETag;
        Depth--;
    }

    public override void WriteProcessingInstruction(scoped RefNameValuePair<byte> pi)
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
    public override void WriteProcessingInstruction(scoped RefNameValuePair<char> pi)
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

    public override void WriteText(scoped ReadOnlySpan<byte> content, bool asCDATA = false)
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
    public override void WriteText(scoped ReadOnlySpan<char> content, bool asCDATA = false)
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

    public override void WriteXMLDecl(scoped RefXMLDecl<byte> decl)
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
    public override void WriteXMLDecl(scoped RefXMLDecl<char> decl)
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
    public override void Flush()
    {
        stream.Flush();
    }

    public void Dispose()
    {
        if (!leaveOpen)
            stream.Dispose();
        GC.SuppressFinalize(this);
    }
}