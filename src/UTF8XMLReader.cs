using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;
using FurinaXML.Parser;
using System;
using System.Text;

namespace FurinaXML;

public ref struct UTF8XMLReader(ReadOnlySpan<byte> utf8data) : IXMLReader<byte>
{
    private ReadOnlySpan<byte> data = utf8data.StartsWith("\uFEFF"u8) ? utf8data[3..] : utf8data;
    private RefXMLDecl<byte> xmlDecl;
    private RefXMLDocTypeDecl<byte> doctypeDecl;
    private ReadOnlySpan<byte> buffer;
    private RefNameValuePair<byte> nvp;
    private bool xmlDeclParsed = false;
    private bool doctypeDeclParsed = false;
    public int Depth { get; private set; } = 0;
    public int Position { get; private set; } = 0;
    public TokenType CurrentToken { get; private set; } = TokenType.None;
    public TokenStatus CurrentTokenStatus { get; private set; } = TokenStatus.None;

    public readonly RefXMLDecl<byte> GetRefXMLDecl()
    {
        if (CurrentToken is TokenType.XMLDecl)
            return xmlDecl;
        else
            throw new InvalidOperationException();
    }
    public readonly XMLDecl GetXMLDecl()
    {
        return new(GetRefXMLDecl());
    }
    public readonly RefXMLDocTypeDecl<byte> GetRefDocTypeDecl()
    {
        if (CurrentToken is TokenType.DocTypeDecl)
            return doctypeDecl;
        else
            throw new InvalidOperationException();
    }
    public readonly XMLDocTypeDecl GetDocTypeDecl()
    {
        return new(GetRefDocTypeDecl());
    }
    public readonly ReadOnlySpan<byte> GetSpan()
    {
        if (CurrentToken
            is TokenType.Comment
            or TokenType.Text
            or TokenType.STagStart
            or TokenType.ETag)
            return buffer;
        else
            throw new InvalidOperationException();
    }
    public readonly string GetString()
    {
        return GetString(XMLParser.DefaultByteNameLookUp);
    }
    public readonly string GetString(ByteNameLookUpDelegate nameMappings)
    {
        if (CurrentToken is not TokenType.Text)
            return Encoding.UTF8.GetString(GetSpan());
        if (!XMLParser.TryNormalize(buffer, out string? output, nameMappings, true))
            throw new InvalidDataException();
        return output;
    }
    public readonly RefNameValuePair<byte> GetUnresolvedRefAttribute()
    {
        if (CurrentToken is TokenType.Attribute)
            return nvp;
        else
            throw new InvalidOperationException();
    }
    public readonly XMLAttribute GetAttribute(bool isCDATA)
    {
        return GetAttribute(XMLParser.DefaultByteNameLookUp, isCDATA);
    }
    public readonly XMLAttribute GetAttribute(ByteNameLookUpDelegate nameMappings, bool isCDATA)
    {
        return new(nvp, nameMappings, isCDATA);
    }
    public readonly RefNameValuePair<byte> GetRefProcessingInstruction()
    {
        if (CurrentToken is TokenType.ProcessingInstruction)
            return nvp;
        else
            throw new InvalidOperationException();
    }
    public readonly XMLProcessingInstruction GetProcessingInstruction()
    {
        return new(nvp);
    }

    public bool Read()
    {
        if (data.IsEmpty)
        {
            CurrentToken = TokenType.None;
            return false;
        }
        if (CurrentToken is TokenType.None or TokenType.STagEnd)
            CurrentTokenStatus = TokenStatus.FirstChild;
        else
            CurrentTokenStatus = TokenStatus.None;
        bool parseResult;
        int consumed;
        if (Depth == 0)
        {
            if (!xmlDeclParsed && data.StartsWith("<?"u8))
            {
                xmlDeclParsed = true;
                parseResult = XMLParser.TryParseRefXMLDecl(data, out xmlDecl, out consumed);
                Position += consumed;
                if (!parseResult)
                    throw XMLException.CreateXMLParsingException(Position, "XMLDecl");
                data = data[consumed..];
                CurrentToken = TokenType.XMLDecl;
                goto END;
            }
            if (!doctypeDeclParsed && data.StartsWith("<!D"u8))
            {
                doctypeDeclParsed = true;
                parseResult = XMLParser.TryParseRefDocTypeDecl(data, out doctypeDecl, out consumed);
                Position += consumed;
                if (!parseResult)
                    throw XMLException.CreateXMLParsingException(Position, "DocTypeDecl");
                data = data[consumed..];
                CurrentToken = TokenType.DocTypeDecl;
                goto END;
            }
        }
        switch (CurrentToken)
        {
            case TokenType.STagStart:
            case TokenType.Attribute:
                consumed = XMLParser.ConsumeWhiteSpace(data);
                Position += consumed;
                data = data[consumed..];
                if (XMLParser.TryParseSTagEnd(data, out bool isEmpty, out consumed))
                {
                    Position += consumed;
                    data = data[consumed..];
                    if (isEmpty)
                    {
                        CurrentToken = TokenType.EmptyTag;
                        Depth--;
                    }
                    else
                        CurrentToken = TokenType.STagEnd;
                    break;
                }
                parseResult = XMLParser.TryParseRefAttributeNoResolve(data, out nvp, out consumed);
                Position += consumed;
                if (!parseResult)
                    throw XMLException.CreateXMLParsingException(Position, "Attribute");
                data = data[consumed..];
                CurrentToken = TokenType.Attribute;
                break;
            default:
                if (data.Length > 2 && data[0] is (byte)'<')
                {
                    switch (data[1])
                    {
                        case (byte)'!':
                            switch (data[2])
                            {
                                case (byte)'-':
                                    parseResult = XMLParser.TryParseComment(data, out buffer, out consumed);
                                    Position += consumed;
                                    if (!parseResult)
                                        throw XMLException.CreateXMLParsingException(Position, "Comment");
                                    CurrentToken = TokenType.Comment;
                                    break;
                                case (byte)'[':
                                    parseResult = XMLParser.TryParseCDATA(data, out buffer, out consumed);
                                    Position += consumed;
                                    if (!parseResult)
                                        throw XMLException.CreateXMLParsingException(Position, "CDATA");
                                    CurrentToken = TokenType.CDATA;
                                    break;
                                default:
                                    throw XMLException.CreateXMLParsingException(Position);
                            }
                            break;
                        case (byte)'?':
                            parseResult = XMLParser.TryParseProcessingInstruction(data, out nvp, out consumed);
                            Position += consumed;
                            if (!parseResult)
                                throw XMLException.CreateXMLParsingException(Position, "ProcessingInstruction");
                            CurrentToken = TokenType.ProcessingInstruction;
                            break;
                        case (byte)'/':
                            parseResult = XMLParser.TryParseETag(data, out buffer, out consumed);
                            Position += consumed;
                            if (!parseResult)
                                throw XMLException.CreateXMLParsingException(Position, "ETag");
                            CurrentToken = TokenType.ETag;
                            Depth--;
                            break;
                        default:
                            parseResult = XMLParser.TryParseSTagStart(data, out buffer, out consumed);
                            Position += consumed;
                            if (!parseResult)
                                throw XMLException.CreateXMLParsingException(Position, "STag");
                            CurrentToken = TokenType.STagStart;
                            Depth++;
                            break;
                    }
                    data = data[consumed..];
                }
                else
                {
                    consumed = data.IndexOf((byte)'<');
                    if (consumed < 0)
                    {
                        if (Depth != 0)
                            throw XMLException.CreateXMLParsingException(Position);
                        consumed = data.Length;
                        CurrentTokenStatus |= TokenStatus.LastChild;
                    }
                    buffer = data[..consumed];
                    Position += consumed;
                    data = data[consumed..];
                    CurrentToken = TokenType.Text;
                }
                break;
        }
    END:
        if (data.StartsWith("</"u8))
            CurrentTokenStatus |= TokenStatus.LastChild;
        return true;
    }
}
