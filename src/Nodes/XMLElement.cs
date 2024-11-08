
namespace FurinaXML.Nodes;

public class XMLElement : IXMLNode
{
    public string Name { get; set; }
    public List<XMLAttribute> Attributes { get; set; }
    public XMLElement? Parent { get; set; }
    public List<IXMLNode> Children { get; } = [];
    public string InnerText
    {
        get => string.Concat(Children.Select(x => x.InnerText));
        set
        {
            Children.Clear();
            Children.Add(new XMLTextNode(value));
        }
    }

    public XMLElement() : this("#document", []) { }
    public XMLElement(string name, params scoped ReadOnlySpan<XMLAttribute> attributes)
    {
        Name = name;
        Attributes = [.. attributes];
    }
    public XMLElement(string name, params IEnumerable<XMLAttribute> attributes)
    {
        Name = name;
        Attributes = [.. attributes];
    }

    public void AppendChild(IXMLNode child)
    {
        child.Parent?.RemoveChild(child);
        Children.Add(child);
        child.Parent = this;
    }
    public bool RemoveChild(IXMLNode child)
    {
        bool result = Children.Remove(child);
        if (result)
            child.Parent = null;
        return result;
    }
}