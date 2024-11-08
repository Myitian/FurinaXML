namespace FurinaXML.Parser;

public partial class XMLParser
{
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-STag"/>
    /// </summary>
    public static bool TryParseSTagStart(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!TryTakeFirst(ref input, out char tmp) || tmp is not '<')
            return false;
        if (!TryParseName(input, out output, out consumedInput))
            return false;
        consumedInput++;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-STag"/>
    /// </summary>
    public static bool TryParseSTagStart(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!TryTakeFirst(ref input, out byte tmp) || tmp is not (byte)'<')
            return false;
        if (!TryParseName(input, out output, out consumedInput))
            return false;
        consumedInput++;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-STag"/>
    /// </summary>
    public static bool TryParseSTagEnd(scoped ReadOnlySpan<char> input, out bool isEmptyTag, out int consumedInput)
    {
        isEmptyTag = false;
        consumedInput = ConsumeWhiteSpace(input);
        input = input[consumedInput..];
        if (input.StartsWith("/>"))
        {
            isEmptyTag = true;
            consumedInput += 2;
            return true;
        }
        else if (input.StartsWith(">"))
        {
            consumedInput++;
            return true;
        }
        return false;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-STag"/>
    /// </summary>
    public static bool TryParseSTagEnd(scoped ReadOnlySpan<byte> input, out bool isEmptyTag, out int consumedInput)
    {
        isEmptyTag = false;
        consumedInput = ConsumeWhiteSpace(input);
        input = input[consumedInput..];
        if (input.StartsWith("/>"u8))
        {
            isEmptyTag = true;
            consumedInput += 2;
            return true;
        }
        else if (input.StartsWith(">"u8))
        {
            consumedInput++;
            return true;
        }
        return false;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-STag"/>
    /// </summary>
    public static bool TryParseETag(ReadOnlySpan<char> input, out ReadOnlySpan<char> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!input.StartsWith("</"))
            return false;
        consumedInput += 2;
        input = input[2..];
        if (!TryParseName(input, out output, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        consumed = ConsumeWhiteSpace(input);
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(input, out char tmp) || tmp is not '>')
            return false;
        consumedInput++;
        return true;
    }
    /// <summary>
    /// <see href="https://www.w3.org/TR/xml/#NT-STag"/>
    /// </summary>
    public static bool TryParseETag(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> output, out int consumedInput)
    {
        output = [];
        consumedInput = 0;
        if (!input.StartsWith("</"u8))
            return false;
        consumedInput += 2;
        input = input[2..];
        if (!TryParseName(input, out output, out int consumed))
            return false;
        consumedInput += consumed;
        input = input[consumed..];
        consumed = ConsumeWhiteSpace(input);
        consumedInput += consumed;
        input = input[consumed..];
        if (!TryTakeFirst(input, out byte tmp) || tmp is not (byte)'>')
            return false;
        consumedInput++;
        return true;
    }
}