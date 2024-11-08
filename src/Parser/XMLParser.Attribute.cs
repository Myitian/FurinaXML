using FurinaXML.Nodes;
using FurinaXML.Nodes.Ref;
using System.Buffers;
using System.Text;

namespace FurinaXML.Parser;

public partial class XMLParser
{
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static bool TryParseAttribute(scoped ReadOnlySpan<char> input, CharNameLookUpDelegate nameMappings, out XMLAttribute attribute, out int consumedInput, bool isCDATA)
    {
        attribute = new();
        consumedInput = 0;
        if (!TryParseName(input, out ReadOnlySpan<char> name, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryParseAttValueNoResolve(input, out ReadOnlySpan<char> value, out consumed, out bool noChange))
            return false;
        consumedInput += consumed;
        attribute.Name = new(name);
        if (noChange)
            attribute.Value = new(value);
        else
        {
            int i = GetNormalizedLength(value, nameMappings, out _, isCDATA);
            Span<char> buffer = stackalloc char[i];
            if (!TryNormalize(value, buffer, nameMappings, out _, isCDATA))
                return false;
            attribute.Value = new(buffer);
        }
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static bool TryParseAttribute(scoped ReadOnlySpan<byte> input, ByteNameLookUpDelegate nameMappings, out XMLAttribute attribute, out int consumedInput, bool isCDATA)
    {
        attribute = new();
        consumedInput = 0;
        if (!TryParseName(input, out ReadOnlySpan<byte> name, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryParseAttValueNoResolve(input, out ReadOnlySpan<byte> value, out consumed, out bool noChange))
            return false;
        consumedInput += consumed;
        attribute.Name = Encoding.UTF8.GetString(name);
        if (noChange)
            attribute.Value = Encoding.UTF8.GetString(value);
        else
        {
            int i = GetNormalizedLength(value, nameMappings, out _, isCDATA);
            Span<byte> buffer = stackalloc byte[i];
            if (!TryNormalize(value, buffer, nameMappings, out _, isCDATA))
                return false;
            attribute.Value = Encoding.UTF8.GetString(buffer);
        }
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Attribute"/>
    /// </summary>
    public static bool TryParseRefAttributeNoResolve(ReadOnlySpan<char> input, out RefNameValuePair<char> attribute, out int consumedInput)
    {
        attribute = new();
        consumedInput = 0;
        if (!TryParseName(input, out attribute.Name, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryParseAttValueNoResolve(input, out attribute.Value, out consumed, out _))
            return false;
        consumedInput += consumed;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Attribute"/>
    /// </summary>
    public static bool TryParseRefAttributeNoResolve(ReadOnlySpan<byte> input, out RefNameValuePair<byte> attribute, out int consumedInput)
    {
        attribute = new();
        consumedInput = 0;
        if (!TryParseName(input, out attribute.Name, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryConsumeEq(input, out consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryParseAttValueNoResolve(input, out attribute.Value, out consumed, out _))
            return false;
        consumedInput += consumed;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static int GetAttValueLengthNoResolve(ReadOnlySpan<char> input, out int consumedInput, out bool noChangesDuringNormalization)
    {
        noChangesDuringNormalization = true;
        consumedInput = 0;
        int consumedOutput = 0;
        if (!TryTakeFirst(ref input, out char quote) || quote is not ('"' or '\''))
            return -1;

        consumedInput++;

        while (true)
        {
            OperationStatus status = Rune.DecodeFromUtf16(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                break;

            if (result.Value is '<')
                return -1;
            else if (result.Value == quote)
            {
                consumedInput += consumed;
                return consumedOutput;
            }
            else if (result.Value is '&')
            {
                int i = GetReferenceLengthNoResolve(input, out consumed);
                if (i < 0)
                    return -1;
                noChangesDuringNormalization = false;
                consumedInput += consumed;
                input = input[consumed..];
                consumedOutput += i;
            }
            else
            {
                if (result.Value is '\t' or '\n' or '\r')
                    noChangesDuringNormalization = false;
                consumedInput += consumed;
                input = input[consumed..];
                consumedOutput += result.Utf16SequenceLength;
            }
        }
        return consumedOutput;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static int GetAttValueLengthNoResolve(scoped ReadOnlySpan<byte> input, out int consumedInput, out bool noChangesDuringNormalization)
    {
        noChangesDuringNormalization = true;
        consumedInput = 0;
        int consumedOutput = 0;
        if (!TryTakeFirst(ref input, out byte quote) || quote is not ((byte)'"' or (byte)'\''))
            return -1;

        consumedInput++;

        while (true)
        {
            OperationStatus status = Rune.DecodeFromUtf8(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                break;

            if (result.Value is '<')
                return -1;
            else if (result.Value == quote)
            {
                consumedInput += consumed;
                return consumedOutput;
            }
            else if (result.Value is '&')
            {
                int i = GetReferenceLengthNoResolve(input, out consumed);
                if (i < 0)
                    return -1;
                noChangesDuringNormalization = false;
                consumedInput += consumed;
                input = input[consumed..];
                consumedOutput += i;
            }
            else
            {
                if (result.Value is '\t' or '\n' or '\r')
                    noChangesDuringNormalization = false;
                consumedInput += consumed;
                input = input[consumed..];
                consumedOutput += result.Utf8SequenceLength;
            }
        }
        return consumedOutput;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static int GetAttValueLength(scoped ReadOnlySpan<char> input, CharNameLookUpDelegate nameMappings, out int consumedInput, out bool noChangesDuringNormalization)
    {
        if (!TryParseAttValueNoResolve(input, out ReadOnlySpan<char> value, out consumedInput, out noChangesDuringNormalization))
            return -1;
        return noChangesDuringNormalization ? value.Length : GetNormalizedLength(value, nameMappings, out _, false);
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static int GetAttValueLength(scoped ReadOnlySpan<byte> input, ByteNameLookUpDelegate nameMappings, out int consumedInput, out bool noChangesDuringNormalization)
    {
        if (!TryParseAttValueNoResolve(input, out ReadOnlySpan<byte> value, out consumedInput, out noChangesDuringNormalization))
            return -1;
        return noChangesDuringNormalization ? value.Length : GetNormalizedLength(value, nameMappings, out _, false);
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static bool TryParseAttValueNoResolve(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput, out bool noChangesDuringNormalization)
    {
        output = [];
        consumedInput = 0;
        noChangesDuringNormalization = true;
        if (!TryTakeFirst(ref input, out char quote) || quote is not ('"' or '\''))
            return false;

        ReadOnlySpan<char> start = input;
        consumedInput++;

        while (true)
        {
            OperationStatus status = Rune.DecodeFromUtf16(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                break;

            if (result.Value is '<')
                return false;
            else if (result.Value == quote)
            {
                output = start[..(consumedInput - 1)];
                consumedInput += consumed;
                return true;
            }
            else if (result.Value is '&')
            {
                int i = GetReferenceLengthNoResolve(input, out consumed);
                if (i < 0)
                    return false;
                noChangesDuringNormalization = false;
                consumedInput += consumed;
                input = input[consumed..];
            }
            else
            {
                if (result.Value is '\t' or '\r' or '\n')
                    noChangesDuringNormalization = false;
                consumedInput += consumed;
                input = input[consumed..];
            }
        }
        return false;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static bool TryParseAttValueNoResolve(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput, out bool noChangesDuringNormalization)
    {
        output = [];
        consumedInput = 0;
        noChangesDuringNormalization = true;
        if (!TryTakeFirst(ref input, out byte quote) || quote is not ((byte)'"' or (byte)'\''))
            return false;

        ReadOnlySpan<byte> start = input;
        consumedInput++;

        while (true)
        {
            OperationStatus status = Rune.DecodeFromUtf8(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                break;

            if (result.Value is '<')
                return false;
            else if (result.Value == quote)
            {
                output = start[..(consumedInput - 1)];
                consumedInput += consumed;
                return true;
            }
            else if (result.Value is '&')
            {
                int i = GetReferenceLengthNoResolve(input, out consumed);
                if (i < 0)
                    return false;
                noChangesDuringNormalization = false;
                consumedInput += consumed;
                input = input[consumed..];
            }
            else
            {
                if (result.Value is '\t' or '\r' or '\n')
                    noChangesDuringNormalization = false;
                consumedInput += consumed;
                input = input[consumed..];
            }
        }
        return false;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static bool TryParseAttValue(scoped ReadOnlySpan<char> input, Span<char> output, CharNameLookUpDelegate nameMappings, out int consumedInput, out int consumedOutput)
    {
        consumedOutput = 0;
        if (!TryParseAttValueNoResolve(input, out ReadOnlySpan<char> value, out consumedInput, out bool noChangesDuringNormalization))
            return false;
        if (noChangesDuringNormalization)
        {
            value.CopyTo(output);
            consumedOutput = value.Length;
            return true;
        }
        else
        {
            return TryNormalize(value, output, nameMappings, out consumedOutput, true);
        }
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-AttValue"/>
    /// </summary>
    public static bool TryParseAttValue(scoped ReadOnlySpan<byte> input, Span<byte> output, ByteNameLookUpDelegate nameMappings, out int consumedInput, out int consumedOutput)
    {
        consumedOutput = 0;
        if (!TryParseAttValueNoResolve(input, out ReadOnlySpan<byte> value, out consumedInput, out bool noChangesDuringNormalization))
            return false;
        if (noChangesDuringNormalization)
        {
            value.CopyTo(output);
            consumedOutput = value.Length;
            return true;
        }
        else
        {
            return TryNormalize(value, output, nameMappings, out consumedOutput, true);
        }
    }
}