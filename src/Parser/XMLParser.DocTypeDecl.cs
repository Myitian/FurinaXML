using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FurinaXML.Parser;

public partial class XMLParser
{
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-doctypedecl"/>
    /// </summary>
    public static bool TryParseDocTypeDecl(scoped ReadOnlySpan<char> input, [MaybeNullWhen(false)] out XMLDocTypeDecl output, out int consumedInput)
    {
        output = null;
        if (!TryParseRefDocTypeDecl(input, out RefXMLDocTypeDecl<char> decl, out consumedInput))
            return false;
        output = new(decl);
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-doctypedecl"/>
    /// </summary>
    public static bool TryParseDocTypeDecl(scoped ReadOnlySpan<byte> input, [MaybeNullWhen(false)] out XMLDocTypeDecl output, out int consumedInput)
    {
        output = null;
        if (!TryParseRefDocTypeDecl(input, out RefXMLDocTypeDecl<byte> decl, out consumedInput))
            return false;
        output = new(decl);
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-doctypedecl"/>
    /// </summary>
    public static bool TryParseRefDocTypeDecl(ReadOnlySpan<char> input, out RefXMLDocTypeDecl<char> output, out int consumedInput)
    {
        output = new();
        consumedInput = 0;
        if (!input.StartsWith("<!DOCTYPE"))
            return false;
        consumedInput += 9;
        input = input[9..];
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryParseName(input, out output.Name, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        consumed = ConsumeWhiteSpace(input);
        consumedInput += consumed;
        input = input[consumed..];
        if (consumed > 0)
        {
            if (!TryParseExternalID(input, ref output, out consumed))
                return false;
            consumedInput += consumed;
            input = input[consumed..];
            consumed = ConsumeWhiteSpace(input);
            consumedInput += consumed;
            input = input[consumed..];
        }
        // Internal Subset is not supported here.
        if (!input.StartsWith(">"))
            return false;
        consumedInput++;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-doctypedecl"/>
    /// </summary>
    public static bool TryParseRefDocTypeDecl(ReadOnlySpan<byte> input, out RefXMLDocTypeDecl<byte> output, out int consumedInput)
    {
        output = new();
        consumedInput = 0;
        if (!input.StartsWith("<!DOCTYPE"u8))
            return false;
        consumedInput += 9;
        input = input[9..];
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryParseName(input, out output.Name, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        consumed = ConsumeWhiteSpace(input);
        consumedInput += consumed;
        input = input[consumed..];
        if (consumed > 0)
        {
            if (!TryParseExternalID(input, ref output, out consumed))
                return false;
            consumedInput += consumed;
            input = input[consumed..];
            consumed = ConsumeWhiteSpace(input);
            consumedInput += consumed;
            input = input[consumed..];
        }
        // Internal Subset is not supported here.
        if (!input.StartsWith(">"u8))
            return false;
        consumedInput++;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-ExternalID"/>
    /// </summary>
    public static bool TryParseExternalID(ReadOnlySpan<char> input, ref RefXMLDocTypeDecl<char> output, out int consumedInput)
    {
        int consumed;
        consumedInput = 0;
        if (input.StartsWith("SYSTEM"))
        {
            output.HasExternalID = true;
            output.IsExternalIDPublic = false;
            consumedInput += 6;
            input = input[6..];
        }
        else if (input.StartsWith("PUBLIC"))
        {
            output.HasExternalID = true;
            output.IsExternalIDPublic = true;
            consumedInput += 6;
            input = input[6..];
            if (!TryConsumeWhiteSpaceAtLeastOne(input, out consumed))
                return false;
            consumedInput += consumed;
            input = input[consumed..];
            if (!TryParsePubIDLiteral(input, out output.PubIDLiteral, out consumed))
                return false;
            consumedInput += consumed;
            input = input[consumed..];
        }
        else
        {
            output.HasExternalID = false;
            output.IsExternalIDPublic = false;
            return true;
        }
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryParseSystemLiteral(input, out output.SystemLiteral, out consumed))
            return false;
        consumedInput += consumed;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-ExternalID"/>
    /// </summary>
    public static bool TryParseExternalID(ReadOnlySpan<byte> input, ref RefXMLDocTypeDecl<byte> output, out int consumedInput)
    {
        int consumed;
        consumedInput = 0;
        if (input.StartsWith("SYSTEM"u8))
        {
            output.HasExternalID = true;
            output.IsExternalIDPublic = false;
            consumedInput += 6;
            input = input[6..];
        }
        else if (input.StartsWith("PUBLIC"u8))
        {
            output.HasExternalID = true;
            output.IsExternalIDPublic = true;
            consumedInput += 6;
            input = input[6..];
            if (!TryConsumeWhiteSpaceAtLeastOne(input, out consumed))
                return false;
            consumedInput += consumed;
            input = input[consumed..];
            if (!TryParsePubIDLiteral(input, out output.PubIDLiteral, out consumed))
                return false;
            consumedInput += consumed;
            input = input[consumed..];
        }
        else
        {
            output.HasExternalID = false;
            output.IsExternalIDPublic = false;
            return true;
        }
        if (!TryConsumeWhiteSpaceAtLeastOne(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryParseSystemLiteral(input, out output.SystemLiteral, out consumed))
            return false;
        consumedInput += consumed;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-PubidLiteral"/>
    /// </summary>
    public static bool TryParsePubIDLiteral(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!TryTakeFirst(ref input, out char quote) || quote is not ('"' or '\''))
            return false;

        ReadOnlySpan<char> start = input;
        consumedInput++;

        while (true)
        {
            OperationStatus status = Rune.DecodeFromUtf16(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                break;

            if (result.Value == quote)
            {
                output = start[..(consumedInput - 1)];
                consumedInput += consumed;
                return true;
            }
            else if (XMLPartValidator.ValidatePubIDChar(result.Value))
            {
                consumedInput += consumed;
                input = input[consumed..];
            }
            else
                break;
        }
        return false;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-PubidLiteral"/>
    /// </summary>
    public static bool TryParsePubIDLiteral(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!TryTakeFirst(ref input, out byte quote) || quote is not ((byte)'"' or (byte)'\''))
            return false;

        ReadOnlySpan<byte> start = input;
        consumedInput++;

        while (true)
        {
            OperationStatus status = Rune.DecodeFromUtf8(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                break;

            if (result.Value == quote)
            {
                output = start[..(consumedInput - 1)];
                consumedInput += consumed;
                return true;
            }
            else if (XMLPartValidator.ValidatePubIDChar(result.Value))
            {
                consumedInput += consumed;
                input = input[consumed..];
            }
            else
                break;
        }
        return false;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-SystemLiteral"/>
    /// </summary>
    public static bool TryParseSystemLiteral(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!TryTakeFirst(ref input, out char quote) || quote is not ('"' or '\''))
            return false;

        ReadOnlySpan<char> start = input;
        consumedInput++;

        while (true)
        {
            OperationStatus status = Rune.DecodeFromUtf16(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                break;

            if (result.Value == quote)
            {
                output = start[..(consumedInput - 1)];
                consumedInput += consumed;
                return true;
            }
            else
            {
                input = input[consumed..];
                consumedInput += consumed;
            }
        }
        return false;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-SystemLiteral"/>
    /// </summary>
    public static bool TryParseSystemLiteral(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!TryTakeFirst(ref input, out byte quote) || quote is not ((byte)'"' or (byte)'\''))
            return false;

        ReadOnlySpan<byte> start = input;
        consumedInput++;

        while (true)
        {
            OperationStatus status = Rune.DecodeFromUtf8(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                break;

            consumedInput += consumed;
            if (result.Value == quote)
            {
                output = start[..(consumedInput - 1)];
                consumedInput += consumed;
                return true;
            }
            else
            {
                input = input[consumed..];
                consumedInput += consumed;
            }
        }
        return false;
    }
}