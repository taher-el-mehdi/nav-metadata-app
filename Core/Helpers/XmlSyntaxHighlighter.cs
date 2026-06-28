using System.Text;
using NAVMetadata.Constants;

namespace NAVMetadata.Helpers;

/// <summary>
/// Applies XML syntax colors via RTF — fast and works with a read-only editor.
/// </summary>
public static class XmlSyntaxHighlighter
{
    /// <summary>Skip RTF for very large documents — RichTextBox becomes unusably slow.</summary>
    public const int MaxHighlightChars = 400_000;

    private static readonly Color DefaultColor = Color.FromArgb(30, 30, 30);
    private static readonly Color TagColor = Color.FromArgb(63, 127, 177);
    private static readonly Color AttributeNameColor = Color.FromArgb(127, 127, 127);
    private static readonly Color AttributeValueColor = Color.FromArgb(163, 21, 21);
    private static readonly Color CommentColor = Color.FromArgb(0, 128, 0);

    private readonly record struct ColorSpan(int Start, int Length, Color Color);

    /// <summary>Builds colored RTF and assigns it to the editor.</summary>
    public static void Apply(RichTextBox editor, string xml)
    {
        if (xml.Length > MaxHighlightChars)
            return;

        editor.Rtf = BuildRtf(xml, CollectSpans(xml));
    }

    /// <summary>Highlights on a background thread, then applies RTF on the UI thread.</summary>
    public static async Task ApplyAsync(RichTextBox editor, string xml, CancellationToken cancellationToken = default)
    {
        if (xml.Length > MaxHighlightChars)
            return;

        var rtf = await Task.Run(() => BuildRtf(xml, CollectSpans(xml)), cancellationToken).ConfigureAwait(false);

        void SetRtf()
        {
            if (!editor.IsDisposed)
                editor.Rtf = rtf;
        }

        if (editor.InvokeRequired)
            editor.Invoke(SetRtf);
        else
            SetRtf();
    }

    private static string BuildRtf(string text, List<ColorSpan> spans)
    {
        var colorList = new List<Color> { DefaultColor };
        var colorIndex = new Dictionary<Color, int> { [DefaultColor] = 1 };

        int IndexOf(Color color)
        {
            if (colorIndex.TryGetValue(color, out var idx))
                return idx;

            colorList.Add(color);
            idx = colorList.Count;
            colorIndex[color] = idx;
            return idx;
        }

        foreach (var span in spans)
            IndexOf(span.Color);

        var sb = new StringBuilder(text.Length + spans.Count * 8 + 256);
        sb.Append(@"{\rtf1\ansi\deff0");
        sb.Append(@"{\fonttbl{\f0\fmodern\fcharset0 ");
        sb.Append(EscapeRtfFontName(AppFonts.Mono.FontFamily.Name));
        sb.Append(@";}}");
        sb.Append(@"\f0\fs").Append((int)(AppFonts.Mono.Size * 2));
        sb.Append(@"{\colortbl ;");
        foreach (var color in colorList)
            sb.Append(@"\red").Append(color.R).Append(@"\green").Append(color.G).Append(@"\blue").Append(color.B).Append(';');
        sb.Append('}');

        spans.Sort(static (a, b) => a.Start.CompareTo(b.Start));

        var pos = 0;
        foreach (var span in spans)
        {
            if (span.Start < pos)
                continue;

            if (span.Start > pos)
            {
                sb.Append(@"\cf1 ");
                AppendRtfText(sb, text.AsSpan(pos, span.Start - pos));
            }

            sb.Append(@"\cf").Append(IndexOf(span.Color)).Append(' ');
            AppendRtfText(sb, text.AsSpan(span.Start, span.Length));
            pos = span.Start + span.Length;
        }

        if (pos < text.Length)
        {
            sb.Append(@"\cf1 ");
            AppendRtfText(sb, text.AsSpan(pos, text.Length - pos));
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static string EscapeRtfFontName(string name) =>
        name.Replace("\\", "\\\\", StringComparison.Ordinal);

    private static void AppendRtfText(StringBuilder sb, ReadOnlySpan<char> text)
    {
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '\\':
                    sb.Append(@"\\");
                    break;
                case '{':
                    sb.Append(@"\{");
                    break;
                case '}':
                    sb.Append(@"\}");
                    break;
                case '\r':
                    break;
                case '\n':
                    sb.Append(@"\line ");
                    break;
                case '\t':
                    sb.Append(@"\tab ");
                    break;
                default:
                    if (ch <= 0x7f)
                        sb.Append(ch);
                    else
                        sb.Append(@"\u").Append((int)ch).Append('?');
                    break;
            }
        }
    }

    private static List<ColorSpan> CollectSpans(string text)
    {
        var spans = new List<ColorSpan>();
        var i = 0;

        while (i < text.Length)
        {
            if (TrySkipComment(text, ref i, spans))
                continue;

            if (TrySkipCData(text, ref i))
                continue;

            if (text[i] == '<')
            {
                if (!LooksLikeTag(text, i))
                {
                    i++;
                    continue;
                }

                ParseTag(text, i, spans, out var next);
                i = next > i ? next : i + 1;
                continue;
            }

            i++;
        }

        return spans;
    }

    private static bool LooksLikeTag(string text, int i)
    {
        if (text[i] != '<' || i + 1 >= text.Length)
            return false;

        return text[i + 1] switch
        {
            '!' or '?' or '/' => true,
            var ch when IsNameStartChar(ch) => true,
            _ => false
        };
    }

    private static bool IsNameStartChar(char ch) =>
        char.IsLetter(ch) || ch is '_' or ':';

    private static bool TrySkipComment(string text, ref int i, List<ColorSpan> spans)
    {
        if (i + 3 >= text.Length || text[i] != '<' || text[i + 1] != '!' || text[i + 2] != '-' || text[i + 3] != '-')
            return false;

        var start = i;
        i += 4;
        while (i + 2 < text.Length && !(text[i] == '-' && text[i + 1] == '-' && text[i + 2] == '>'))
            i++;

        i = Math.Min(i + 3, text.Length);
        spans.Add(new ColorSpan(start, i - start, CommentColor));
        return true;
    }

    private static bool TrySkipCData(string text, ref int i)
    {
        const string marker = "<![CDATA[";
        if (i + marker.Length > text.Length || !text.AsSpan(i, marker.Length).SequenceEqual(marker))
            return false;

        i += marker.Length;
        var end = text.IndexOf("]]>", i, StringComparison.Ordinal);
        i = end >= 0 ? end + 3 : text.Length;
        return true;
    }

    private static void ParseTag(string text, int tagOpen, List<ColorSpan> spans, out int next)
    {
        spans.Add(new ColorSpan(tagOpen, 1, TagColor));

        var i = tagOpen + 1;
        if (i < text.Length && text[i] == '/')
        {
            spans.Add(new ColorSpan(i, 1, TagColor));
            i++;
        }
        else if (i < text.Length && text[i] == '?')
        {
            spans.Add(new ColorSpan(i, 1, TagColor));
            i++;
        }

        i = SkipSpaces(text, i);
        var nameStart = i;
        while (i < text.Length && IsNameChar(text[i]))
            i++;

        if (i > nameStart)
            spans.Add(new ColorSpan(nameStart, i - nameStart, TagColor));

        while (i < text.Length && text[i] != '>')
        {
            var loopStart = i;
            i = SkipSpaces(text, i);
            if (i >= text.Length || text[i] == '>' || text[i] == '/')
                break;

            var attrNameStart = i;
            while (i < text.Length && IsNameChar(text[i]))
                i++;

            if (i > attrNameStart)
            {
                spans.Add(new ColorSpan(attrNameStart, i - attrNameStart, AttributeNameColor));

                i = SkipSpaces(text, i);
                if (i < text.Length && text[i] == '=')
                {
                    spans.Add(new ColorSpan(i, 1, AttributeNameColor));
                    i++;

                    i = SkipSpaces(text, i);
                    if (i < text.Length && text[i] is '"' or '\'')
                    {
                        var quote = text[i];
                        var valueStart = i;
                        i = SkipQuotedValue(text, i + 1, quote);
                        if (i < text.Length && text[i] == quote)
                            i++;

                        spans.Add(new ColorSpan(valueStart, i - valueStart, AttributeValueColor));
                    }
                }

                continue;
            }

            // Not a valid attribute — advance to avoid infinite loops on malformed markup.
            i = loopStart + 1;
        }

        if (i < text.Length && text[i] == '/')
        {
            spans.Add(new ColorSpan(i, 1, TagColor));
            i++;
        }

        if (i < text.Length && text[i] == '>')
        {
            spans.Add(new ColorSpan(i, 1, TagColor));
            i++;
        }

        next = i;
    }

    private static int SkipQuotedValue(string text, int i, char quote)
    {
        while (i < text.Length)
        {
            var ch = text[i];
            if (ch == quote)
                return i;

            if (ch == '&')
            {
                var semi = text.IndexOf(';', i + 1);
                i = semi >= 0 ? semi + 1 : i + 1;
                continue;
            }

            i++;
        }

        return i;
    }

    private static int SkipSpaces(string text, int i)
    {
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;
        return i;
    }

    private static bool IsNameChar(char ch) =>
        char.IsLetterOrDigit(ch) || ch is ':' or '.' or '_' or '-';
}
