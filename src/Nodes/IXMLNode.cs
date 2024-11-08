namespace FurinaXML.Nodes;

public interface IXMLNode
{
    string InnerText { get; set; }
    XMLElement? Parent { get; set; }
}