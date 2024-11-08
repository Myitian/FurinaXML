using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;

namespace FurinaXML;

public abstract class AbstractXMLWriter : IXMLWriter
{
    public virtual TokenType LastToken { get; protected set; } = TokenType.None;
    public virtual int Depth { get; protected set; } = 0;

    protected abstract void Write(scoped ReadOnlySpan<byte> utf8data);
    protected abstract void Write(scoped ReadOnlySpan<char> utf16data);
    protected abstract void Write(char ch);

    public abstract void WriteAttribute(scoped RefNameValuePair<byte> attribute);
    public abstract void WriteAttribute(scoped RefNameValuePair<char> attribute);
    public virtual void WriteAttribute(XMLAttribute attribute)
    {
        WriteAttribute(new RefNameValuePair<char>
        {
            Name = attribute.Name,
            Value = attribute.Value
        });
    }

    public virtual void WriteAttributes(params IEnumerable<XMLAttribute>? attributes)
    {
        if (attributes is null)
            return;
        foreach (XMLAttribute attribute in attributes)
            WriteAttribute(attribute);
    }
    public virtual void WriteAttributes(params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        foreach (XMLAttribute attribute in attributes)
            WriteAttribute(attribute);
    }

    public abstract void WriteComment(scoped ReadOnlySpan<byte> content);
    public abstract void WriteComment(scoped ReadOnlySpan<char> content);
    public virtual void WriteComment(XMLComment content)
    {
        WriteComment(content.InnerText);
    }

    public abstract void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<byte> decl);
    public abstract void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<char> decl);
    public virtual void WriteDocTypeDecl(XMLDocTypeDecl decl)
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

    public virtual void WriteElement(XMLElement element)
    {
        WriteSTag(element.Name, element.Attributes);
        foreach (IXMLNode node in element.Children)
            WriteNode(node);
        WriteETag(element.Name);
    }

    public virtual void WriteEmptyTag(scoped ReadOnlySpan<byte> name, params IEnumerable<XMLAttribute>? attributes)
    {
        WriteSTag(name);
        WriteAttributes(attributes);
        WriteSTagEnd(true);
    }
    public virtual void WriteEmptyTag(scoped ReadOnlySpan<char> name, params IEnumerable<XMLAttribute>? attributes)
    {
        WriteSTag(name);
        WriteAttributes(attributes);
        WriteSTagEnd(true);
    }
    public virtual void WriteEmptyTag(scoped ReadOnlySpan<byte> name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        WriteSTag(name);
        WriteAttributes(attributes);
        WriteSTagEnd(true);
    }
    public virtual void WriteEmptyTag(scoped ReadOnlySpan<char> name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        WriteSTag(name);
        WriteAttributes(attributes);
        WriteSTagEnd(true);
    }

    public virtual void WriteEmptyTag(XMLElement element)
    {
        WriteEmptyTag(element.Name, element.Attributes);
    }

    public abstract void WriteETag(scoped ReadOnlySpan<byte> name);
    public abstract void WriteETag(scoped ReadOnlySpan<char> name);

    public virtual void WriteNode(IXMLNode node)
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

    public abstract void WriteProcessingInstruction(scoped RefNameValuePair<byte> pi);
    public abstract void WriteProcessingInstruction(scoped RefNameValuePair<char> pi);
    public virtual void WriteProcessingInstruction(XMLProcessingInstruction pi)
    {
        WriteAttribute(new RefNameValuePair<char>
        {
            Name = pi.Name,
            Value = pi.Value
        });
    }

    public virtual void WriteSTag(scoped ReadOnlySpan<byte> name, params IEnumerable<XMLAttribute>? attributes)
    {
        WriteSTagStart(name);
        WriteAttributes(attributes);
    }
    public virtual void WriteSTag(scoped ReadOnlySpan<char> name, params IEnumerable<XMLAttribute>? attributes)
    {
        WriteSTagStart(name);
        WriteAttributes(attributes);
    }
    public virtual void WriteSTag(scoped ReadOnlySpan<byte> name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        WriteSTagStart(name);
        WriteAttributes(attributes);
    }
    public virtual void WriteSTag(scoped ReadOnlySpan<char> name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        WriteSTagStart(name);
        WriteAttributes(attributes);
    }

    public virtual void WriteSTagEnd(bool isEmptyTag)
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

    public virtual void WriteSTagStart(scoped ReadOnlySpan<byte> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write('<');
        Write(name);
        LastToken = TokenType.STagStart;
        Depth++;
    }
    public virtual void WriteSTagStart(scoped ReadOnlySpan<char> name)
    {
        if (LastToken is TokenType.STagStart or TokenType.Attribute)
            throw new InvalidOperationException();
        Write('<');
        Write(name);
        LastToken = TokenType.STagStart;
        Depth++;
    }

    public abstract void WriteText(scoped ReadOnlySpan<byte> content, bool asCDATA = false);
    public abstract void WriteText(scoped ReadOnlySpan<char> content, bool asCDATA = false);
    public virtual void WriteText(XMLTextNode node, bool asCDATA = false)
    {
        WriteText(node.InnerText, asCDATA);
    }

    public abstract void WriteXMLDecl(scoped RefXMLDecl<byte> decl);
    public abstract void WriteXMLDecl(scoped RefXMLDecl<char> decl);
    public virtual void WriteXMLDecl(XMLDecl decl)
    {
        WriteXMLDecl(new RefXMLDecl<char>
        {
            Version = decl.Version,
            Encoding = decl.Encoding,
            Standalone = decl.Standalone,
        });
    }

    public virtual void WriteLine()
    {
        Write('\n');
    }
    public virtual void WriteSpace(int count)
    {
        while (count-- > 0)
            Write(' ');
    }
    public abstract void Flush();
}