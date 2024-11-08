using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;
using FurinaXML.Parser;

namespace FurinaXML;

public ref struct UTF16XMLReader(ReadOnlySpan<char> utf16data) : IXMLReader<char>
{
    private ReadOnlySpan<char> data = utf16data.StartsWith("\uFEFF") ? utf16data[1..] : utf16data;
    private RefXMLDecl<char> xmlDecl;
    private RefXMLDocTypeDecl<char> doctypeDecl;
    private ReadOnlySpan<char> buffer;
    private RefNameValuePair<char> nvp;
    private bool xmlDeclParsed = false;
    private bool doctypeDeclParsed = false;
    public int Depth { get; private set; } = 0;
    public int Position { get; private set; } = 0;
    public TokenType CurrentToken { get; private set; } = TokenType.None;
    public TokenStatus CurrentTokenStatus { get; private set; } = TokenStatus.None;

    public readonly RefXMLDecl<char> GetRefXMLDecl()
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
    public readonly RefXMLDocTypeDecl<char> GetRefDocTypeDecl()
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
    public readonly ReadOnlySpan<char> GetSpan()
    {
        if (CurrentToken
            is TokenType.Comment
            or TokenType.Text
            or TokenType.CDATA
            or TokenType.STagStart
            or TokenType.ETag)
            return buffer;
        else
            throw new InvalidOperationException();
    }
    public readonly string GetString()
    {
        return GetString(XMLParser.DefaultCharNameLookUp);
    }
    public readonly string GetString(CharNameLookUpDelegate nameMappings)
    {
        if (CurrentToken is not TokenType.Text)
            return new(GetSpan());
        if (!XMLParser.TryNormalize(buffer, out string? output, nameMappings, true))
            throw new InvalidDataException();
        return output;
    }
    public readonly RefNameValuePair<char> GetUnresolvedRefAttribute()
    {
        if (CurrentToken is TokenType.Attribute)
            return nvp;
        else
            throw new InvalidOperationException();
    }
    public readonly XMLAttribute GetAttribute(bool isCDATA)
    {
        return GetAttribute(XMLParser.DefaultCharNameLookUp, isCDATA);
    }
    public readonly XMLAttribute GetAttribute(CharNameLookUpDelegate nameMappings, bool isCDATA)
    {
        return new(nvp, nameMappings, isCDATA);
    }
    public readonly RefNameValuePair<char> GetRefProcessingInstruction()
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
            if (!xmlDeclParsed && data.StartsWith("<?"))
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
            if (!doctypeDeclParsed && data.StartsWith("<!D"))
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
                if (data.Length > 2 && data[0] is '<')
                {
                    switch (data[1])
                    {
                        case '!':
                            switch (data[2])
                            {
                                case '-':
                                    parseResult = XMLParser.TryParseComment(data, out buffer, out consumed);
                                    Position += consumed;
                                    if (!parseResult)
                                        throw XMLException.CreateXMLParsingException(Position, "Comment");
                                    CurrentToken = TokenType.Comment;
                                    break;
                                case '[':
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
                        case '?':
                            parseResult = XMLParser.TryParseProcessingInstruction(data, out nvp, out consumed);
                            Position += consumed;
                            if (!parseResult)
                                throw XMLException.CreateXMLParsingException(Position, "ProcessingInstruction");
                            CurrentToken = TokenType.ProcessingInstruction;
                            break;
                        case '/':
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
                    consumed = data.IndexOf('<');
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
        if (data.StartsWith("</"))
            CurrentTokenStatus |= TokenStatus.LastChild;
        return true;
    }
}
