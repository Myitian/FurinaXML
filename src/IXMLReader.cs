using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;

namespace FurinaXML
{
    public interface IXMLReader
    {
        TokenType CurrentToken { get; }
        TokenStatus CurrentTokenStatus { get; }
        int Depth { get; }
        int Position { get; }

        XMLAttribute GetAttribute(bool isCDATA);
        XMLDocTypeDecl GetDocTypeDecl();
        XMLProcessingInstruction GetProcessingInstruction();
        string GetString();
        XMLDecl GetXMLDecl();
        bool Read();
    }
    public interface IXMLReader<T> : IXMLReader where T : unmanaged
    {
        RefXMLDocTypeDecl<T> GetRefDocTypeDecl();
        RefNameValuePair<T> GetRefProcessingInstruction();
        RefXMLDecl<T> GetRefXMLDecl();
        ReadOnlySpan<T> GetSpan();
        RefNameValuePair<T> GetUnresolvedRefAttribute();
    }
}