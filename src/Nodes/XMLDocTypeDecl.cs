using FurinaXML.Nodes.Ref;
using System.Text;

namespace FurinaXML.Nodes;

public class XMLDocTypeDecl : IXMLNode
{
    public string Name { get; set; }
    public string? PubIDLiteral { get; set; }
    public string? SystemLiteral { get; set; }
    public bool HasExternalID { get; set; }
    public bool IsExternalIDPublic { get; set; }
    public string InnerText
    {
        get => "";
        set => throw new NotSupportedException();
    }
    public XMLElement? Parent { get; set; }

    public XMLDocTypeDecl(string name)
    {
        Name = name;
    }
    public XMLDocTypeDecl(scoped RefXMLDocTypeDecl<char> decl)
    {
        Name = new(decl.Name);
        HasExternalID = decl.HasExternalID;
        IsExternalIDPublic = decl.IsExternalIDPublic;
        SystemLiteral = null;
        PubIDLiteral = null;
        if (HasExternalID)
        {
            SystemLiteral = new(decl.SystemLiteral);
            if (IsExternalIDPublic)
                PubIDLiteral = new(decl.PubIDLiteral);
        }
    }
    public XMLDocTypeDecl(scoped RefXMLDocTypeDecl<byte> decl)
    {
        Name = Encoding.UTF8.GetString(decl.Name);
        HasExternalID = decl.HasExternalID;
        IsExternalIDPublic = decl.IsExternalIDPublic;
        SystemLiteral = null;
        PubIDLiteral = null;
        if (HasExternalID)
        {
            SystemLiteral = Encoding.UTF8.GetString(decl.SystemLiteral);
            if (IsExternalIDPublic)
                PubIDLiteral = Encoding.UTF8.GetString(decl.PubIDLiteral);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new("<!DOCTYPE ");
        sb.Append(Name);
        if (HasExternalID)
        {
            if (IsExternalIDPublic)
                sb.Append(" PUBLIC \"").Append(PubIDLiteral).Append('"');
            else
                sb.Append(" SYSTEM");
            sb.Append(' ');
            if (SystemLiteral?.Contains('"') is true)
                sb.Append('\'').Append(SystemLiteral).Append('\'');
            else
                sb.Append('"').Append(SystemLiteral).Append('"');
        }
        return sb.Append('>').ToString();
    }
}