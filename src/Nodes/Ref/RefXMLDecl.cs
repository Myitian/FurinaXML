namespace FurinaXML.Nodes.Ref;

public ref struct RefXMLDecl<T> where T : unmanaged
{
    public XMLVersion Version;
    public ReadOnlySpan<T> Encoding;
    public bool? Standalone;
}