using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;

namespace FurinaXML;

public interface IXMLWriter
{
    void WriteLine();
    void WriteSpace(int count);
    void WriteXMLDecl(scoped RefXMLDecl<byte> decl);
    void WriteXMLDecl(scoped RefXMLDecl<char> decl);
    void WriteXMLDecl(XMLDecl decl);
    void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<byte> decl);
    void WriteDocTypeDecl(scoped RefXMLDocTypeDecl<char> decl);
    void WriteDocTypeDecl(XMLDocTypeDecl decl);
    void WriteProcessingInstruction(scoped RefNameValuePair<byte> pi);
    void WriteProcessingInstruction(scoped RefNameValuePair<char> pi);
    void WriteProcessingInstruction(XMLProcessingInstruction pi);
    void WriteComment(scoped ReadOnlySpan<byte> content);
    void WriteComment(scoped ReadOnlySpan<char> content);
    void WriteComment(XMLComment content);
    void WriteText(scoped ReadOnlySpan<byte> content, bool asCDATA = false);
    void WriteText(scoped ReadOnlySpan<char> content, bool asCDATA = false);
    void WriteText(XMLTextNode node, bool asCDATA = false);
    void WriteSTagStart(scoped ReadOnlySpan<byte> name);
    void WriteSTagStart(scoped ReadOnlySpan<char> name);
    void WriteSTag(scoped ReadOnlySpan<byte> name, params IEnumerable<XMLAttribute>? attributes);
    void WriteSTag(scoped ReadOnlySpan<char> name, params IEnumerable<XMLAttribute>? attributes);
    void WriteSTag(scoped ReadOnlySpan<byte> name, params scoped ReadOnlySpan<XMLAttribute> attributes);
    void WriteSTag(scoped ReadOnlySpan<char> name, params scoped ReadOnlySpan<XMLAttribute> attributes);
    void WriteAttribute(scoped RefNameValuePair<byte> attribute);
    void WriteAttribute(scoped RefNameValuePair<char> attribute);
    void WriteAttribute(XMLAttribute attribute);
    void WriteAttributes(params IEnumerable<XMLAttribute>? attributes);
    void WriteAttributes(params scoped ReadOnlySpan<XMLAttribute> attributes);
    void WriteSTagEnd(bool isEmptyTag);
    void WriteETag(scoped ReadOnlySpan<byte> name);
    void WriteETag(scoped ReadOnlySpan<char> name);
    void WriteEmptyTag(scoped ReadOnlySpan<byte> name, params IEnumerable<XMLAttribute>? attributes);
    void WriteEmptyTag(scoped ReadOnlySpan<char> name, params IEnumerable<XMLAttribute>? attributes);
    void WriteEmptyTag(scoped ReadOnlySpan<byte> name, params scoped ReadOnlySpan<XMLAttribute> attributes);
    void WriteEmptyTag(scoped ReadOnlySpan<char> name, params scoped ReadOnlySpan<XMLAttribute> attributes);
    void WriteEmptyTag(XMLElement element);
    void WriteElement(XMLElement element);
    void WriteNode(IXMLNode node);
}
