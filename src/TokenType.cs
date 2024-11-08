namespace FurinaXML;

public enum TokenType
{
    None,
    XMLDecl,
    DocTypeDecl,
    ProcessingInstruction,
    Comment,
    STagStart,
    Attribute,
    STagEnd,
    EmptyTag,
    Text,
    CDATA,
    ETag,
}