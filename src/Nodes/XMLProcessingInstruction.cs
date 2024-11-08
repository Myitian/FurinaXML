using FurinaXML.Nodes.Ref;
using System.Collections.Immutable;
using System.Text;

namespace FurinaXML.Nodes;

public struct XMLProcessingInstruction(string name, string value) : IXMLNode
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
    public string InnerText { readonly get => ""; set => throw new NotSupportedException(); }
    public XMLElement? Parent { get; set; }
    public IList<IXMLNode> Children { get; } = ImmutableList<IXMLNode>.Empty;

    public XMLProcessingInstruction(KeyValuePair<string, string> kvp)
        : this(kvp.Key, kvp.Value) { }
    public XMLProcessingInstruction(scoped RefNameValuePair<byte> nvp)
        : this(Encoding.UTF8.GetString(nvp.Name), Encoding.UTF8.GetString(nvp.Value)) { }
    public XMLProcessingInstruction(scoped RefNameValuePair<char> nvp)
        : this(new(nvp.Name), new(nvp.Value)) { }
}