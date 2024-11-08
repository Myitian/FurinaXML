namespace FurinaXML;

[Flags]
public enum TokenStatus
{
    None = 0b00,
    FirstChild = 0b01,
    LastChild = 0b10
}