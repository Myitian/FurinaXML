namespace FurinaXML.Nodes;

public struct XMLTextNode(string text) : IXMLNode
{
    public string InnerText { get; set; } = text;
    public XMLElement? Parent { get; set; }
}