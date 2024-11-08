namespace FurinaXML.Nodes.Ref;

public ref struct RefXMLDocTypeDecl<T> where T : unmanaged
{
    public ReadOnlySpan<T> Name;
    public ReadOnlySpan<T> PubIDLiteral;
    public ReadOnlySpan<T> SystemLiteral;
    public bool HasExternalID;
    public bool IsExternalIDPublic;
}