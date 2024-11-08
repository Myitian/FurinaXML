using FurinaXML.Nodes.Ref;
using System.Text;

namespace FurinaXML.Nodes;

public class XMLDecl(XMLVersion version, string? encoding = null, bool? standalone = null) : IXMLNode
{
    public XMLVersion Version { get; set; } = version;
    public string? Encoding { get; set; } = encoding;
    public bool? Standalone { get; set; } = standalone;
    public string InnerText
    {
        get => "";
        set => throw new NotSupportedException();
    }
    public XMLElement? Parent { get; set; }

    public XMLDecl()
        : this(new(), null) { }
    public XMLDecl(scoped RefXMLDecl<char> decl)
        : this(decl.Version, decl.Encoding.IsEmpty ? null : new(decl.Encoding), decl.Standalone) { }
    public XMLDecl(scoped RefXMLDecl<byte> decl)
        : this(decl.Version, decl.Encoding.IsEmpty ? null : System.Text.Encoding.UTF8.GetString(decl.Encoding), decl.Standalone) { }

    public override string ToString()
    {
        StringBuilder sb = new("<?xml version=\"");
        sb.Append(Version.ToString()).Append('"');
        if (!string.IsNullOrEmpty(Encoding))
            sb.Append(" encoding=\"").Append(Encoding).Append('"');
        if (Standalone is not null)
            sb.Append(" standalone=\"").Append(Standalone.Value ? "yes" : "no").Append('"');
        return sb.Append("?>").ToString();
    }
}