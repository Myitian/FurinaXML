namespace FurinaXML.Nodes.Ref;

public ref struct RefNameValuePair<T> where T : unmanaged
{
    public ReadOnlySpan<T> Name;
    public ReadOnlySpan<T> Value;
}