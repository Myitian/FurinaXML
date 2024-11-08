using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;
using System.Diagnostics.CodeAnalysis;

namespace FurinaXML.Parser;

public partial class XMLParser
{
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-XMLDecl"/>
    /// </summary>
    public static bool TryParseXMLDecl(scoped ReadOnlySpan<char> input, out XMLDecl output, out int consumedInput)
    {
        output = new();
        if (!TryParseRefXMLDecl(input, out RefXMLDecl<char> refDecl, out consumedInput))
            return false;
        output = new(refDecl);
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-XMLDecl"/>
    /// </summary>
    public static bool TryParseXMLDecl(scoped ReadOnlySpan<byte> input, out XMLDecl output, out int consumedInput)
    {
        output = new();
        if (!TryParseRefXMLDecl(input, out RefXMLDecl<byte> refDecl, out consumedInput))
            return false;
        output = new(refDecl);
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-XMLDecl"/>
    /// </summary>
    public static bool TryParseRefXMLDecl(ReadOnlySpan<char> input, out RefXMLDecl<char> output, out int consumedInput)
    {
        consumedInput = 0;
        output = new();
        if (!input.StartsWith("<?xml"))
            return false;
        consumedInput += 5;
        input = input[5..];
        if (!TryParseVersionInfo(input, out output.Version, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (TryParseEncodingDecl(input, out output.Encoding, out consumed))
        {
            consumedInput += consumed;
            input = input[consumed..];
        }
        if (TryParseSDDecl(input, out output.Standalone, out consumed))
        {
            consumedInput += consumed;
            input = input[consumed..];
        }
        consumed = ConsumeWhiteSpace(input);
        consumedInput += consumed;
        input = input[consumed..];
        if (!input.StartsWith("?>"))
            return false;
        consumedInput += 2;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-XMLDecl"/>
    /// </summary>
    public static bool TryParseRefXMLDecl(ReadOnlySpan<byte> input, out RefXMLDecl<byte> output, out int consumedInput)
    {
        consumedInput = 0;
        output = new();
        if (!input.StartsWith("<?xml"u8))
            return false;
        consumedInput += 5;
        input = input[5..];
        if (!TryParseVersionInfo(input, out output.Version, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (TryParseEncodingDecl(input, out output.Encoding, out consumed))
        {
            consumedInput += consumed;
            input = input[consumed..];
        }
        if (TryParseSDDecl(input, out output.Standalone, out consumed))
        {
            consumedInput += consumed;
            input = input[consumed..];
        }
        consumed = ConsumeWhiteSpace(input);
        consumedInput += consumed;
        input = input[consumed..];
        if (!input.StartsWith("?>"u8))
            return false;
        consumedInput += 2;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-VersionInfo"/>
    /// </summary>
    public static bool TryParseVersionInfo(scoped ReadOnlySpan<char> input, out XMLVersion output, out int consumedInput)
    {
        consumedInput = 0;
        output = default;
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!input.StartsWith("version"))
            return false;
        consumedInput += 7;
        input = input[7..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(ref input, out char quote) || quote is not ('"' or '\''))
            return false;
        consumedInput++;
        if (!input.StartsWith("1."))
            return false;
        consumedInput += 2;
        input = input[2..];
        if (!TryParsePositiveInteger(input, false, out int minor, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(ref input, out char tmp) || tmp != quote)
            return false;
        consumedInput++;
        output = new(minor);
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-VersionInfo"/>
    /// </summary>
    public static bool TryParseVersionInfo(scoped ReadOnlySpan<byte> input, out XMLVersion output, out int consumedInput)
    {
        consumedInput = 0;
        output = default;
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!input.StartsWith("version"u8))
            return false;
        consumedInput += 7;
        input = input[7..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(ref input, out byte quote) || quote is not ((byte)'"' or (byte)'\''))
            return false;
        consumedInput++;
        if (!input.StartsWith("1."u8))
            return false;
        consumedInput += 2;
        input = input[2..];
        if (!TryParsePositiveInteger(input, false, out int minor, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(ref input, out byte tmp) || tmp != quote)
            return false;
        consumedInput++;
        output = new(minor);
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-EncodingDecl"/>
    /// </summary>
    public static bool TryParseEncodingDecl(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput)
    {
        consumedInput = 0;
        output = default;
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!input.StartsWith("encoding"))
            return false;
        consumedInput += 8;
        input = input[8..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(ref input, out char quote) || quote is not ('"' or '\''))
            return false;
        consumedInput++;
        int i = input.IndexOf(quote);
        if (i < 0 || !XMLPartValidator.ValidateEncName(input[..i]))
            return false;
        consumedInput += i;
        consumedInput++;
        output = input[..i];
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-EncodingDecl"/>
    /// </summary>
    public static bool TryParseEncodingDecl(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput)
    {
        consumedInput = 0;
        output = default;
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!input.StartsWith("encoding"u8))
            return false;
        consumedInput += 8;
        input = input[8..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(ref input, out byte quote) || quote is not ((byte)'"' or (byte)'\''))
            return false;
        consumedInput++;
        int i = input.IndexOf(quote);
        if (i < 0 || !XMLPartValidator.ValidateEncName(input[..i]))
            return false;
        consumedInput += i;
        consumedInput++;
        output = input[..i];
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-SDDecl"/>
    /// </summary>
    public static bool TryParseSDDecl(scoped ReadOnlySpan<char> input, [MaybeNullWhen(false)] out bool? standalone, out int consumedInput)
    {
        consumedInput = 0;
        standalone = null;
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!input.StartsWith("standalone"))
            return false;
        consumedInput += 10;
        input = input[10..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (input.StartsWith("\"yes\"") || input.StartsWith("'yes'"))
        {
            standalone = true;
            consumedInput += 5;
            return true;
        }
        else if (input.StartsWith("\"no\"") || input.StartsWith("'no'"))
        {
            standalone = false;
            consumedInput += 4;
            return true;
        }
        return false;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-SDDecl"/>
    /// </summary>
    public static bool TryParseSDDecl(scoped ReadOnlySpan<byte> input, [MaybeNullWhen(false)] out bool? standalone, out int consumedInput)
    {
        consumedInput = 0;
        standalone = null;
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!input.StartsWith("standalone"u8))
            return false;
        consumedInput += 10;
        input = input[10..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (input.StartsWith("\"yes\""u8) || input.StartsWith("'yes'"u8))
        {
            standalone = true;
            consumedInput += 5;
            return true;
        }
        else if (input.StartsWith("\"no\""u8) || input.StartsWith("'no'"u8))
        {
            standalone = false;
            consumedInput += 4;
            return true;
        }
        return false;
    }
}