namespace FurinaXML;

public class XMLException : Exception
{
    public int Position { get; }

    public XMLException()
    {
    }
    public XMLException(string? message) : this(message, null)
    {
    }
    public XMLException(int position, string? message) : this(position, message, null)
    {
    }
    public XMLException(string? message, Exception? innerException) : base(message, innerException)
    {
        Position = -1;
    }
    public XMLException(int position, string? message, Exception? innerException) : this($"{message} near position {position}", innerException)
    {
        Position = position;
    }

    public static XMLException CreateXMLParsingException(int position, string name)
    {
        return new XMLException(position, $"Cannot parse {name}");
    }
    public static XMLException CreateXMLParsingException(int position)
    {
        return new XMLException(position, "Invalid content");
    }
    public static XMLException CreateXMLUnableToResolveReferenceException()
    {
        return new XMLException("Unable to resolve reference");
    }
}