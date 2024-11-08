namespace FurinaXML.Nodes;

public class XMLComment(string text) : IXMLNode
{
    public string InnerText { get; set; } = text;
    public XMLElement? Parent { get; set; }
}