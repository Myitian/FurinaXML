using FurinaXML.Nodes.Ref;
using System.Diagnostics.CodeAnalysis;

namespace FurinaXML.Parser;

public delegate bool CharNameLookUpDelegate(scoped ReadOnlySpan<char> text, out ReadOnlySpan<char> result);
public delegate bool ByteNameLookUpDelegate(scoped ReadOnlySpan<byte> text, out ReadOnlySpan<byte> result);
public partial class XMLParser
{
    public static bool DefaultCharNameLookUp(scoped ReadOnlySpan<char> text, out ReadOnlySpan<char> result)
    {
        switch (text)
        {
            case "lt":
                result = "<";
                return true;
            case "gt":
                result = ">";
                return true;
            case "amp":
                result = "&";
                return true;
            case "apos":
                result = "'";
                return true;
            case "quot":
                result = "\"";
                return true;
            default:
                result = [];
                return false;
        }
    }
    public static bool DefaultByteNameLookUp(scoped ReadOnlySpan<byte> text, out ReadOnlySpan<byte> result)
    {
        switch (text.Length)
        {
            case 2 when text.SequenceEqual("lt"u8):
                result = "<"u8;
                return true;
            case 2 when text.SequenceEqual("gt"u8):
                result = ">"u8;
                return true;
            case 3 when text.SequenceEqual("amp"u8):
                result = "&"u8;
                return true;
            case 4 when text.SequenceEqual("apos"u8):
                result = "'"u8;
                return true;
            case 4 when text.SequenceEqual("quot"u8):
                result = "\""u8;
                return true;
            default:
                result = [];
                return false;
        }
    }

    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Comment"/>
    /// </summary>
    public static bool TryParseComment(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!input.StartsWith("<!--"))
            return false;
        consumedInput += 4;
        input = input[4..];
        int i = input.IndexOf("-->");
        if (i == -1)
            return false;
        output = input[..i];
        consumedInput += i;
        consumedInput += 3;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Comment"/>
    /// </summary>
    public static bool TryParseComment(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!input.StartsWith("<!--"u8))
            return false;
        consumedInput += 4;
        input = input[4..];
        int i = input.IndexOf("-->"u8);
        if (i == -1)
            return false;
        output = input[..i];
        consumedInput += i;
        consumedInput += 3;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Comment"/>
    /// </summary>
    public static bool TryParseCDATA(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!input.StartsWith("<![CDATA["))
            return false;
        consumedInput += 4;
        input = input[4..];
        int i = input.IndexOf("]]>");
        if (i == -1)
            return false;
        output = input[..i];
        consumedInput += i;
        consumedInput += 3;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Comment"/>
    /// </summary>
    public static bool TryParseCDATA(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!input.StartsWith("<![CDATA["u8))
            return false;
        consumedInput += 9;
        input = input[9..];
        int i = input.IndexOf("]]>"u8);
        if (i == -1)
            return false;
        output = input[..i];
        consumedInput += i;
        consumedInput += 3;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-PI"/>
    /// </summary>
    public static bool TryParseProcessingInstruction(ReadOnlySpan<char> input, out RefNameValuePair<char> output, out int consumedInput)
    {
        output = new();
        consumedInput = 0;
        if (!input.StartsWith("<?"))
            return false;
        consumedInput += 2;
        input = input[2..];
        if (!TryParseName(input, out output.Name, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        int i = input.IndexOf("?>");
        if (i == -1)
            return false;
        output.Value = input[..i];
        consumedInput += i;
        consumedInput += 2;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-PI"/>
    /// </summary>
    public static bool TryParseProcessingInstruction(ReadOnlySpan<byte> input, out RefNameValuePair<byte> output, out int consumedInput)
    {
        output = new();
        consumedInput = 0;
        if (!input.StartsWith("<?"u8))
            return false;
        consumedInput += 2;
        input = input[2..];
        if (!TryParseName(input, out output.Name, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        int i = input.IndexOf("?>"u8);
        if (i == -1)
            return false;
        output.Value = input[..i];
        consumedInput += i;
        consumedInput += 2;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Eq"/>
    /// </summary>
    public static bool TryConsumeEq(scoped ReadOnlySpan<char> input, out int consumed)
    {
        consumed = ConsumeWhiteSpace(input);
        input = input[consumed..];
        if (!TryTakeFirst(ref input, out char eq) || eq is not '=')
            return false;
        consumed++;
        int consumed2 = ConsumeWhiteSpace(input);
        consumed += consumed2;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Eq"/>
    /// </summary>
    public static bool TryConsumeEq(scoped ReadOnlySpan<byte> input, out int consumed)
    {
        consumed = ConsumeWhiteSpace(input);
        input = input[consumed..];
        if (!TryTakeFirst(ref input, out byte eq) || eq is not (byte)'=')
            return false;
        consumed++;
        int consumed2 = ConsumeWhiteSpace(input);
        consumed += consumed2;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-S"/>
    /// </summary>
    public static int ConsumeWhiteSpace(scoped ReadOnlySpan<char> input)
    {
        int consumed = 0;
        while (TryTakeFirst(ref input, out char tmp) && XMLPartValidator.ValidateWhiteSpace(tmp))
            consumed++;
        return consumed;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-S"/>
    /// </summary>
    public static int ConsumeWhiteSpace(scoped ReadOnlySpan<byte> input)
    {
        int consumed = 0;
        while (TryTakeFirst(ref input, out byte tmp) && XMLPartValidator.ValidateWhiteSpace(tmp))
            consumed++;
        return consumed;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-S"/>
    /// </summary>
    public static bool TryConsumeWhiteSpaceAtLeastOne(scoped ReadOnlySpan<char> input, out int consumed)
    {
        consumed = ConsumeWhiteSpace(input);
        return consumed > 0;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-S"/>
    /// </summary>
    public static bool TryConsumeWhiteSpaceAtLeastOne(scoped ReadOnlySpan<byte> input, out int consumed)
    {
        consumed = ConsumeWhiteSpace(input);
        return consumed > 0;
    }

    public static bool TryTakeFirst<T>(scoped ref ReadOnlySpan<T> span, [MaybeNullWhen(false)] out T value)
    {
        if (span.IsEmpty)
        {
            value = default;
            return false;
        }
        else
        {
            value = span[0];
            span = span[1..];
            return true;
        }
    }
    public static bool TryTakeFirst<T>(scoped ReadOnlySpan<T> span, [MaybeNullWhen(false)] out T value)
    {
        if (span.IsEmpty)
        {
            value = default;
            return false;
        }
        else
        {
            value = span[0];
            return true;
        }
    }
    public static bool TryParsePositiveInteger(scoped ReadOnlySpan<char> input, bool hexadecimal, out int value, out int consumed)
    {
        int length0 = input.Length;
        value = 0;
        if (hexadecimal)
        {
            while (TryTakeFirst(ref input, out char tmp))
            {
                if (tmp is >= '0' and <= '9')
                    value = value << 4 | tmp ^ '0';
                else if (tmp is >= 'A' and <= 'F')
                    value = value << 4 | tmp - ('A' - 10);
                else if (tmp is >= 'a' and <= 'f')
                    value = value << 4 | tmp - ('a' - 10);
                else
                {
                    length0--;
                    break;
                }
            }
        }
        else
        {
            while (TryTakeFirst(ref input, out char tmp))
            {
                if (tmp is >= '0' and <= '9')
                    value = value * 10 + (tmp ^ '0');
                else
                {
                    length0--;
                    break;
                }
            }
        }
        consumed = length0 - input.Length;
        return consumed > 0;
    }
    public static bool TryParsePositiveInteger(scoped ReadOnlySpan<byte> input, bool hexadecimal, out int value, out int consumed)
    {
        int length0 = input.Length;
        value = 0;
        if (hexadecimal)
        {
            while (TryTakeFirst(ref input, out byte tmp))
            {
                if (tmp is >= (byte)'0' and <= (byte)'9')
                    value = value << 4 | tmp ^ '0';
                else if (tmp is >= (byte)'A' and <= (byte)'F')
                    value = value << 4 | tmp - ('A' - 10);
                else if (tmp is >= (byte)'a' and <= (byte)'f')
                    value = value << 4 | tmp - ('a' - 10);
                else
                {
                    length0--;
                    break;
                }
            }
        }
        else
        {
            while (TryTakeFirst(ref input, out byte tmp))
            {
                if (tmp is >= (byte)'0' and <= (byte)'9')
                    value = value * 10 + (tmp ^ '0');
                else
                {
                    length0--;
                    break;
                }
            }
        }
        consumed = length0 - input.Length;
        return consumed > 0;
    }
}