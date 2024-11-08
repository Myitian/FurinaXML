using System.Buffers;
using System.Text;

namespace FurinaXML.Parser;

public partial class XMLParser
{
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Name"/>
    /// </summary>
    public static int GetNameLength(scoped ReadOnlySpan<char> input, out int consumedInput)
    {
        consumedInput = 0;
        OperationStatus status = Rune.DecodeFromUtf16(input, out Rune result, out int consumed);
        if (status is not OperationStatus.Done || !XMLPartValidator.ValidateNameStartChar(result.Value))
            return -1;
        do
        {
            consumedInput += consumed;
            input = input[consumed..];
            status = Rune.DecodeFromUtf16(input, out result, out consumed);
        } while (status is OperationStatus.Done && XMLPartValidator.ValidateNameChar(result.Value));
        return consumedInput;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Name"/>
    /// </summary>
    public static int GetNameLength(scoped ReadOnlySpan<byte> input, out int consumedInput)
    {
        consumedInput = 0;
        OperationStatus status = Rune.DecodeFromUtf8(input, out Rune result, out int consumed);
        if (status is not OperationStatus.Done || !XMLPartValidator.ValidateNameStartChar(result.Value))
            return -1;
        do
        {
            consumedInput += consumed;
            input = input[consumed..];
            status = Rune.DecodeFromUtf8(input, out result, out consumed);
        } while (status is OperationStatus.Done && XMLPartValidator.ValidateNameChar(result.Value));
        return consumedInput;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Name"/>
    /// </summary>
    public static bool TryParseName(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput)
    {
        consumedInput = 0;
        output = [];
        OperationStatus status = Rune.DecodeFromUtf16(input, out Rune result, out int consumed);
        if (status is not OperationStatus.Done || !XMLPartValidator.ValidateNameStartChar(result.Value))
            return false;
        ReadOnlySpan<char> nameStart = input;
        do
        {
            consumedInput += consumed;
            input = input[consumed..];
            status = Rune.DecodeFromUtf16(input, out result, out consumed);
        } while (status is OperationStatus.Done && XMLPartValidator.ValidateNameChar(result.Value));
        output = nameStart[..consumedInput];
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Name"/>
    /// </summary>
    public static bool TryParseName(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput)
    {
        consumedInput = 0;
        output = [];
        OperationStatus status = Rune.DecodeFromUtf8(input, out Rune result, out int consumed);
        if (status is not OperationStatus.Done || !XMLPartValidator.ValidateNameStartChar(result.Value))
            return false;
        ReadOnlySpan<byte> nameStart = input;
        do
        {
            consumedInput += consumed;
            input = input[consumed..];
            status = Rune.DecodeFromUtf8(input, out result, out consumed);
        } while (status is OperationStatus.Done && XMLPartValidator.ValidateNameChar(result.Value));
        output = nameStart[..consumedInput];
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Name"/>
    /// </summary>
    public static bool TryParseName(scoped ReadOnlySpan<char> input, Span<char> output, out int consumedInput, out int consumedOutput)
    {
        consumedOutput = 0;
        if (!TryParseName(input, out ReadOnlySpan<char> slice, out consumedInput))
            return false;
        slice.CopyTo(output);
        consumedOutput = consumedInput;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-Name"/>
    /// </summary>
    public static bool TryParseName(scoped ReadOnlySpan<byte> input, Span<byte> output, out int consumedInput, out int consumedOutput)
    {
        consumedOutput = 0;
        if (!TryParseName(input, out ReadOnlySpan<byte> slice, out consumedInput))
            return false;
        slice.CopyTo(output);
        consumedOutput = consumedInput;
        return true;
    }
}