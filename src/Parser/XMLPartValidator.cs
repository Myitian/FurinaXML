using System.Buffers;
using System.Text;

namespace FurinaXML.Parser;

internal partial class XMLPartValidator
{
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-S"/>
    /// </summary>
    public static bool ValidateWhiteSpace(int utf32char)
    {
        return utf32char is '\t' or '\n' or '\r' or ' ';
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-NameStartChar"/>
    /// </summary>
    public static bool ValidateNameStartChar(int utf32char)
    {
        return utf32char
            is ':'
            or (>= 'A' and <= 'Z')
            or '_'
            or (>= 'a' and <= 'z')
            or (>= '\xC0' and <= '\xD6')
            or (>= '\xD8' and <= '\xF6')
            or (>= '\xF8' and <= '\x2FF')
            or (>= '\x370' and <= '\x37D')
            or (>= '\x37F' and <= '\x1FFF')
            or (>= '\x200C' and <= '\x200D')
            or (>= '\x2070' and <= '\x218F')
            or (>= '\x2C00' and <= '\x2FEF')
            or (>= '\x3001' and <= '\xD7FF')
            or (>= '\xF900' and <= '\xFDCF')
            or (>= '\xFDF0' and <= '\xFFFD')
            or (>= 0x10000 and <= 0xEFFFF);
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-NameChar"/>
    /// </summary>
    public static bool ValidateNameChar(int utf32char)
    {
        return utf32char
            is '-'
            or '.'
            or (>= '0' and <= ':')
            or (>= 'A' and <= 'Z')
            or '_'
            or (>= 'a' and <= 'z')
            or '\xB7'
            or (>= '\xC0' and <= '\xD6')
            or (>= '\xD8' and <= '\xF6')
            or (>= '\xF8' and <= '\x2FF')
            or (>= '\x300' and <= '\x37D')
            or (>= '\x37F' and <= '\x1FFF')
            or (>= '\x200C' and <= '\x200D')
            or (>= '\x203F' and <= '\x2040')
            or (>= '\x2070' and <= '\x218F')
            or (>= '\x2C00' and <= '\x2FEF')
            or (>= '\x3001' and <= '\xD7FF')
            or (>= '\xF900' and <= '\xFDCF')
            or (>= '\xFDF0' and <= '\xFFFD')
            or (>= 0x10000 and <= 0xEFFFF);
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Name"/>
    /// </summary>
    public static bool ValidateName(scoped ReadOnlySpan<char> name)
    {
        OperationStatus status = Rune.DecodeFromUtf16(name, out Rune result, out int consumed);
        if (status is not OperationStatus.Done || !ValidateNameStartChar(result.Value))
            return false;
        name = name[consumed..];
        while (!name.IsEmpty)
        {
            status = Rune.DecodeFromUtf16(name, out result, out consumed);
            if (status is not OperationStatus.Done || !ValidateNameChar(result.Value))
                return false;
            name = name[consumed..];
        }
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Name"/>
    /// </summary>
    public static bool ValidateName(scoped ReadOnlySpan<byte> name)
    {
        OperationStatus status = Rune.DecodeFromUtf8(name, out Rune result, out int consumed);
        if (status is not OperationStatus.Done || !ValidateNameStartChar(result.Value))
            return false;
        name = name[consumed..];
        while (!name.IsEmpty)
        {
            status = Rune.DecodeFromUtf8(name, out result, out consumed);
            if (status is not OperationStatus.Done || !ValidateNameChar(result.Value))
                return false;
            name = name[consumed..];
        }
        return true;
    }

    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-EncName"/>
    /// </summary>
    public static bool ValidateEncNameStart(ushort c) => c
        is (>= 'A' and <= 'Z')
        or (>= 'a' and <= 'z');
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-EncName"/>
    /// </summary>
    public static bool ValidateEncNameMiddle(ushort c) => c
        is (>= 'A' and <= 'Z')
        or (>= 'a' and <= 'z')
        or (>= '0' and <= '9')
        or '_'
        or '.'
        or '-';
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-EncName"/>
    /// </summary>
    public static bool ValidateEncNameEnd(ushort c) => c
        is (>= 'A' and <= 'Z')
        or (>= 'a' and <= 'z')
        or (>= '0' and <= '9')
        or '_'
        or '.';
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-EncName"/>
    /// </summary>
    public static bool ValidateEncName(scoped ReadOnlySpan<char> name)
    {
        if (!XMLParser.TryTakeFirst(ref name, out char tmp) || !ValidateEncNameStart(tmp))
            return false;
        while (name.Length > 1)
        {
            if (!XMLParser.TryTakeFirst(ref name, out tmp) || !ValidateEncNameMiddle(tmp))
                return false;
        }
        if (!XMLParser.TryTakeFirst(ref name, out tmp) || !ValidateEncNameEnd(tmp))
            return false;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-EncName"/>
    /// </summary>
    public static bool ValidateEncName(scoped ReadOnlySpan<byte> name)
    {
        if (!XMLParser.TryTakeFirst(ref name, out byte tmp) || !ValidateEncNameStart(tmp))
            return false;
        while (name.Length > 1)
        {
            if (!XMLParser.TryTakeFirst(ref name, out tmp) || !ValidateEncNameMiddle(tmp))
                return false;
        }
        if (!XMLParser.TryTakeFirst(ref name, out tmp) || !ValidateEncNameEnd(tmp))
            return false;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-PubidChar"/>
    /// </summary>
    public static bool ValidatePubIDChar(int utf32char)
    {
        return utf32char
            is '\n'
            or '\r'
            or ' '
            or '!'
            or (>= '#' and <= '%')
            or (>= '\'' and <= ';')
            or '='
            or (>= '?' and <= 'Z')
            or '_'
            or (>= 'a' and <= 'z');
    }
}
