using FurinaXML.Nodes.Ref;
using FurinaXML.Parser;
using System.Text;

namespace FurinaXML.Nodes;

public struct XMLAttribute(string name, string value)
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;

    public XMLAttribute(KeyValuePair<string, string> kvp)
        : this(kvp.Key, kvp.Value) { }

    public XMLAttribute(scoped RefNameValuePair<byte> nvp, ByteNameLookUpDelegate nameMappings, bool isCDATA)
        : this(
              Encoding.UTF8.GetString(nvp.Name),
              XMLParser.TryNormalize(
                  nvp.Value,
                  out string? value,
                  nameMappings,
                  isCDATA) ? value : throw XMLException.CreateXMLUnableToResolveReferenceException())
    { }

    public XMLAttribute(scoped RefNameValuePair<char> nvp, CharNameLookUpDelegate nameMappings, bool isCDATA)
        : this(
              new(nvp.Name),
              XMLParser.TryNormalize(
                  nvp.Value,
                  out string? value,
                  nameMappings,
                  isCDATA) ? value : throw XMLException.CreateXMLUnableToResolveReferenceException())
    { }

    public override string ToString()
    {
        StringBuilder sb = new(Name);
        sb.Append("=\"");
        foreach (char c in Value)
        {
            switch (c)
            {
                case '"':
                    sb.Append("&quot;");
                    break;
                case '&':
                    sb.Append("&amp;");
                    break;
                case '<':
                    sb.Append("&lt;");
                    break;
                default:
                    if (char.IsControl(c))
                        sb.Append($"&#{(int)c:X};");
                    else
                        sb.Append(c);
                    break;
            }
        }
        return sb.Append('"').ToString();
    }
}