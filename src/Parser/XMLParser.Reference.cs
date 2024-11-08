using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FurinaXML.Parser;

public partial class XMLParser
{
    private static readonly Rune SpaceRune = new(' ');

    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Reference"/>
    /// </summary>
    public static int GetReferenceLengthNoResolve(scoped ReadOnlySpan<char> input, out int consumedInput)
    {
        consumedInput = 0;
        if (!TryTakeFirst(ref input, out char tmp) || tmp is not '&')
            return -1;
        consumedInput++;
        if (!TryTakeFirst(input, out tmp))
            return -1;

        if (tmp is '#')
        {
            consumedInput++;
            if (!TryParseCharRefBody(input[1..], out _, out int consumed))
                return -1;
            consumedInput += consumed;
        }
        else
        {
            int semicolon = input.IndexOf(';');
            if (semicolon <= 0)
                return -1;
            int i = GetNameLength(input, out int consumed);
            if (i < 0 || consumed != semicolon)
                return -1;
            consumedInput += semicolon + 1;
        }
        int consumedOutput = consumedInput;
        return consumedOutput;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Reference"/>
    /// </summary>
    public static int GetReferenceLengthNoResolve(scoped ReadOnlySpan<byte> input, out int consumedInput)
    {
        consumedInput = 0;
        if (!TryTakeFirst(ref input, out byte tmp) || tmp is not (byte)'&')
            return -1;
        consumedInput++;
        if (!TryTakeFirst(input, out tmp))
            return -1;

        if (tmp is (byte)'#')
        {
            consumedInput++;
            if (!TryParseCharRefBody(input[1..], out _, out int consumed))
                return -1;
            consumedInput += consumed;
        }
        else
        {
            int semicolon = input.IndexOf((byte)';');
            if (semicolon <= 0)
                return -1;
            int i = GetNameLength(input, out int consumed);
            if (i < 0 || consumed != semicolon)
                return -1;
            consumedInput += semicolon + 1;
        }
        int consumedOutput = consumedInput;
        return consumedOutput;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Reference"/>
    /// </summary>
    public static int GetReferenceLength(scoped ReadOnlySpan<char> input, CharNameLookUpDelegate nameMappings, out int consumedInput)
    {
        ArgumentNullException.ThrowIfNull(nameof(nameMappings));

        consumedInput = 0;
        int consumedOutput = 0;

        if (!TryTakeFirst(ref input, out char tmp) || tmp is not '&')
            return -1;
        consumedInput++;
        if (!TryTakeFirst(input, out tmp))
            return -1;

        if (tmp is '#')
        {
            consumedInput++;
            if (!TryParseCharRefBody(input[1..], out Rune result, out int consumed))
                return -1;
            int len = result.Utf16SequenceLength;
            consumedInput += consumed;
            consumedOutput += len;
        }
        else
        {
            int semicolon = input.IndexOf(';');
            if (semicolon <= 0)
                return -1;
            if (semicolon <= 2048)
            {
                Span<char> nameBuffer = stackalloc char[semicolon];
                if (!TryParseName(input, nameBuffer, out int consumed, out int consumedNameOutput)
                    || consumed != semicolon
                    || !nameMappings(nameBuffer[..consumed], out ReadOnlySpan<char> result))
                    return -1;
                consumedOutput += result.Length;
            }
            else
            {
                char[] arr = ArrayPool<char>.Shared.Rent(semicolon);
                Span<char> nameBuffer = arr.AsSpan(0, semicolon);
                if (!TryParseName(input, nameBuffer, out int consumed, out int consumedNameOutput)
                    || consumed != semicolon
                    || !nameMappings(nameBuffer[..consumed], out ReadOnlySpan<char> result))
                    return -1;
                consumedOutput += result.Length;
            }
            consumedInput += semicolon + 1;
        }
        return consumedOutput;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Reference"/>
    /// </summary>
    public static int GetReferenceLength(scoped ReadOnlySpan<byte> input, ByteNameLookUpDelegate nameMappings, out int consumedInput)
    {
        ArgumentNullException.ThrowIfNull(nameof(nameMappings));

        consumedInput = 0;
        int consumedOutput = 0;

        if (!TryTakeFirst(ref input, out byte tmp) || tmp is not (byte)'&')
            return -1;
        consumedInput++;
        if (!TryTakeFirst(input, out tmp))
            return -1;

        if (tmp is (byte)'#')
        {
            consumedInput++;
            if (!TryParseCharRefBody(input[1..], out Rune result, out int consumed))
                return -1;
            int len = result.Utf16SequenceLength;
            consumedInput += consumed;
            consumedOutput += len;
        }
        else
        {
            int semicolon = input.IndexOf((byte)';');
            if (semicolon <= 0)
                return -1;
            if (semicolon <= 2048)
            {
                Span<byte> nameBuffer = stackalloc byte[semicolon];
                if (!TryParseName(input, nameBuffer, out int consumed, out int consumedNameOutput)
                    || consumed != semicolon
                    || !nameMappings(nameBuffer[..consumed], out ReadOnlySpan<byte> result))
                    return -1;
                consumedOutput += result.Length;
            }
            else
            {
                byte[] arr = ArrayPool<byte>.Shared.Rent(semicolon);
                Span<byte> nameBuffer = arr.AsSpan(0, semicolon);
                if (!TryParseName(input, nameBuffer, out int consumed, out int consumedNameOutput)
                    || consumed != semicolon
                    || !nameMappings(nameBuffer[..consumed], out ReadOnlySpan<byte> result))
                    return -1;
                consumedOutput += result.Length;
            }
            consumedInput += semicolon + 1;
        }
        return consumedOutput;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Reference"/>
    /// </summary>
    public static bool TryParseReference(scoped ReadOnlySpan<char> input, Span<char> output, CharNameLookUpDelegate nameMappings, out int consumedInput, out int consumedOutput)
    {
        ArgumentNullException.ThrowIfNull(nameof(nameMappings));

        consumedInput = 0;
        consumedOutput = 0;

        if (!TryTakeFirst(ref input, out char tmp) || tmp is not '&')
            return false;
        consumedInput++;
        if (!TryTakeFirst(input, out tmp))
            return false;

        if (tmp is '#')
        {
            consumedInput++;
            if (!TryParseCharRefBody(input[1..], out Rune result, out int consumed))
                return false;
            consumedInput += consumed;
            consumedOutput += result.EncodeToUtf16(output);
        }
        else
        {
            int semicolon = input.IndexOf(';');
            if (semicolon <= 0)
                return false;
            if (semicolon <= 2048)
            {
                Span<char> nameBuffer = stackalloc char[semicolon];
                if (!TryParseName(input, nameBuffer, out int consumed, out int consumedNameOutput)
                    || consumed != semicolon
                    || !nameMappings(nameBuffer[..consumed], out ReadOnlySpan<char> result))
                    return false;
                result.CopyTo(output);
                consumedOutput += result.Length;
            }
            else
            {
                char[] arr = ArrayPool<char>.Shared.Rent(semicolon);
                Span<char> nameBuffer = arr.AsSpan(0, semicolon);
                if (!TryParseName(input, nameBuffer, out int consumed, out int consumedNameOutput)
                    || consumed != semicolon
                    || !nameMappings(nameBuffer[..consumed], out ReadOnlySpan<char> result))
                    return false;
                result.CopyTo(output);
                consumedOutput += result.Length;
            }
            consumedInput += semicolon + 1;
        }
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Reference"/>
    /// </summary>
    public static bool TryParseReference(scoped ReadOnlySpan<byte> input, Span<byte> output, ByteNameLookUpDelegate nameMappings, out int consumedInput, out int consumedOutput)
    {
        ArgumentNullException.ThrowIfNull(nameof(nameMappings));

        consumedInput = 0;
        consumedOutput = 0;

        if (!TryTakeFirst(ref input, out byte tmp) || tmp is not (byte)'&')
            return false;
        consumedInput++;
        if (!TryTakeFirst(input, out tmp))
            return false;

        if (tmp is (byte)'#')
        {
            consumedInput++;
            if (!TryParseCharRefBody(input[1..], out Rune result, out int consumed))
                return false;
            consumedInput += consumed;
            consumedOutput += result.EncodeToUtf8(output);
        }
        else
        {
            int semicolon = input.IndexOf((byte)';');
            if (semicolon <= 0)
                return false;
            if (semicolon <= 2048)
            {
                Span<byte> nameBuffer = stackalloc byte[semicolon];
                if (!TryParseName(input, nameBuffer, out int consumed, out int consumedNameOutput)
                    || consumed != semicolon
                    || !nameMappings(nameBuffer[..consumed], out ReadOnlySpan<byte> result))
                    return false;
                result.CopyTo(output);
                consumedOutput += result.Length;
            }
            else
            {
                byte[] arr = ArrayPool<byte>.Shared.Rent(semicolon);
                Span<byte> nameBuffer = arr.AsSpan(0, semicolon);
                if (!TryParseName(input, nameBuffer, out int consumed, out int consumedNameOutput)
                    || consumed != semicolon
                    || !nameMappings(nameBuffer[..consumed], out ReadOnlySpan<byte> result))
                    return false;
                result.CopyTo(output);
                consumedOutput += result.Length;
            }
            consumedInput += semicolon + 1;
        }
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-CharRef"/>
    /// </summary>
    public static bool TryParseCharRefBody(scoped ReadOnlySpan<char> input, out Rune output, out int consumedInput)
    {
        consumedInput = 0;
        output = Rune.ReplacementChar;
        if (!TryTakeFirst(input, out char tmp))
            return false;
        bool hex = false;
        if (tmp is 'x')
        {
            consumedInput++;
            input = input[1..];
            hex = true;
        }
        if (!TryParsePositiveInteger(input, hex, out int outputInt, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(input, out tmp) || tmp is not ';')
            return false;
        consumedInput++;
        return Rune.TryCreate(outputInt, out output);
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-CharRef"/>
    /// </summary>
    public static bool TryParseCharRefBody(scoped ReadOnlySpan<byte> input, out Rune output, out int consumedInput)
    {
        consumedInput = 0;
        output = Rune.ReplacementChar;
        if (!TryTakeFirst(input, out byte tmp))
            return false;
        bool hex = false;
        if (tmp is (byte)'x')
        {
            consumedInput++;
            input = input[1..];
            hex = true;
        }
        if (!TryParsePositiveInteger(input, hex, out int outputInt, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(input, out tmp) || tmp is not (byte)';')
            return false;
        consumedInput++;
        return Rune.TryCreate(outputInt, out output);
    }
    public static int GetNormalizedLength(scoped ReadOnlySpan<char> input, CharNameLookUpDelegate nameMappings, out bool noChangesDuringNormalization, bool isCDATA)
    {
        noChangesDuringNormalization = true;
        int consumedOutput = 0;
        if (!isCDATA)
        {
            int consumed = ConsumeWhiteSpace(input);
            input = input[consumed..];
        }
        while (!input.IsEmpty)
        {
            OperationStatus status = Rune.DecodeFromUtf16(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                return -1;

            if (result.Value is '&')
            {
                int i = GetReferenceLength(input, nameMappings, out consumed);
                if (i < 0)
                    return -1;
                noChangesDuringNormalization = false;
                input = input[consumed..];
                consumedOutput += i;
            }
            else
            {
                if (!isCDATA)
                {
                    int consumedS = ConsumeWhiteSpace(input);
                    if (consumedS > 0)
                    {
                        char c0 = input[0];
                        input = input[consumedS..];
                        if (input.IsEmpty)
                            break;
                        if (consumedS > 1 || c0 is not ' ')
                            noChangesDuringNormalization = false;
                        result = SpaceRune;
                        consumed = consumedS;
                    }
                }
                else if (result.Value is '\t' or '\r' or '\n')
                {
                    noChangesDuringNormalization = false;
                    result = SpaceRune;
                }
                input = input[consumed..];
                consumedOutput += result.Utf16SequenceLength;
            }
        }
        return consumedOutput;
    }
    public static int GetNormalizedLength(scoped ReadOnlySpan<byte> input, ByteNameLookUpDelegate nameMappings, out bool noChangesDuringNormalization, bool isCDATA)
    {
        noChangesDuringNormalization = true;
        int consumedOutput = 0;
        if (!isCDATA)
        {
            int consumed = ConsumeWhiteSpace(input);
            input = input[consumed..];
        }
        while (!input.IsEmpty)
        {
            OperationStatus status = Rune.DecodeFromUtf8(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                return -1;

            if (result.Value is '&')
            {
                int i = GetReferenceLength(input, nameMappings, out consumed);
                if (i < 0)
                    return -1;
                noChangesDuringNormalization = false;
                input = input[consumed..];
                consumedOutput += i;
            }
            else
            {
                if (!isCDATA)
                {
                    consumed = ConsumeWhiteSpace(input);
                    if (consumed > 0)
                    {
                        byte c0 = input[0];
                        input = input[consumed..];
                        if (input.IsEmpty)
                            break;
                        if (consumed > 1 || c0 is not (byte)' ')
                            noChangesDuringNormalization = false;
                        result = SpaceRune;
                    }
                }
                else if (result.Value is '\t' or '\r' or '\n')
                {
                    noChangesDuringNormalization = false;
                    result = SpaceRune;
                }
                input = input[consumed..];
                consumedOutput += result.Utf16SequenceLength;
            }
        }
        return consumedOutput;
    }
    public static bool TryNormalize(scoped ReadOnlySpan<char> input, Span<char> output, CharNameLookUpDelegate nameMappings, out int consumedOutput, bool isCDATA)
    {
        consumedOutput = 0;
        if (!isCDATA)
        {
            int consumed = ConsumeWhiteSpace(input);
            input = input[consumed..];
        }
        while (!input.IsEmpty)
        {
            OperationStatus status = Rune.DecodeFromUtf16(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                return false;

            if (result.Value is '&')
            {
                if (!TryParseReference(input, output, nameMappings, out consumed, out int refConsumedOutput))
                    return false;
                input = input[consumed..];
                output = output[refConsumedOutput..];
                consumedOutput += refConsumedOutput;
            }
            else
            {
                if (!isCDATA)
                {
                    consumed = ConsumeWhiteSpace(input);
                    if (consumed > 0)
                    {
                        input = input[consumed..];
                        if (input.IsEmpty)
                            break;
                        result = SpaceRune;
                    }
                }
                else if (result.Value is '\t' or '\r' or '\n')
                    result = SpaceRune;
                input = input[consumed..];
                int len = result.EncodeToUtf16(output);
                output = output[len..];
                consumedOutput += len;
            }
        }
        return true;
    }
    public static bool TryNormalize(scoped ReadOnlySpan<byte> input, Span<byte> output, ByteNameLookUpDelegate nameMappings, out int consumedOutput, bool isCDATA)
    {
        consumedOutput = 0;
        if (!isCDATA)
        {
            int consumed = ConsumeWhiteSpace(input);
            input = input[consumed..];
        }
        while (!input.IsEmpty)
        {
            OperationStatus status = Rune.DecodeFromUtf8(input, out Rune result, out int consumed);
            if (status is not OperationStatus.Done)
                return false;

            if (result.Value is '&')
            {
                if (!TryParseReference(input, output, nameMappings, out consumed, out int refConsumedOutput))
                    return false;
                input = input[consumed..];
                output = output[refConsumedOutput..];
                consumedOutput += refConsumedOutput;
            }
            else
            {
                if (!isCDATA)
                {
                    consumed = ConsumeWhiteSpace(input);
                    if (consumed > 0)
                    {
                        input = input[consumed..];
                        if (input.IsEmpty)
                            break;
                        result = SpaceRune;
                    }
                }
                else if (result.Value is '\t' or '\r' or '\n')
                    result = SpaceRune;
                input = input[consumed..];
                int len = result.EncodeToUtf8(output);
                output = output[len..];
                consumedOutput += len;
            }
        }
        return true;
    }
    public static bool TryNormalize(scoped ReadOnlySpan<char> input, [MaybeNullWhen(false)] out string output, CharNameLookUpDelegate nameMappings, bool isCDATA)
    {
        output = null;
        int len = GetNormalizedLength(input, nameMappings, out bool noChange, isCDATA);
        if (len < 0)
            return false;
        if (noChange)
            output = new(input);
        else
        {
            if (len <= 2048)
            {
                Span<char> buffer = stackalloc char[len];
                if (!TryNormalize(input, buffer, nameMappings, out _, isCDATA))
                    return false;
                output = new(buffer);
            }
            else
            {
                char[] arr = ArrayPool<char>.Shared.Rent(len);
                Span<char> buffer = arr.AsSpan(0, len);
                if (!TryNormalize(input, buffer, nameMappings, out _, isCDATA))
                    return false;
                output = new(buffer);
            }
        }
        return true;
    }
    public static bool TryNormalize(scoped ReadOnlySpan<byte> input, [MaybeNullWhen(false)] out string output, ByteNameLookUpDelegate nameMappings, bool isCDATA)
    {
        output = null;
        int len = GetNormalizedLength(input, nameMappings, out bool noChange, isCDATA);
        if (len < 0)
            return false;
        if (noChange)
            output = Encoding.UTF8.GetString(input);
        else
        {
            if (len <= 2048)
            {
                Span<byte> buffer = stackalloc byte[len];
                if (!TryNormalize(input, buffer, nameMappings, out _, isCDATA))
                    return false;
                output = Encoding.UTF8.GetString(buffer);
            }
            else
            {
                byte[] arr = ArrayPool<byte>.Shared.Rent(len);
                Span<byte> buffer = arr.AsSpan(0, len);
                if (!TryNormalize(input, buffer, nameMappings, out _, isCDATA))
                    return false;
                output = Encoding.UTF8.GetString(buffer);
            }
        }
        return true;
    }
}