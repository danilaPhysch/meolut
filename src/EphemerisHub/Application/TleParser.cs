using System.Text.RegularExpressions;
using EphemerisHub.Models;

namespace EphemerisHub.Application;

public static partial class TleParser
{
    private const string UnknownName = "Unknown";

    public static IEnumerable<ParsedTle> ParseLines(List<string> lines)
    {
        var i = 0;

        while (i < lines.Count)
        {
            if (lines[i].StartsWith("1 ") && i + 1 < lines.Count && lines[i + 1].StartsWith("2 "))
            {
                var line1 = lines[i];
                var line2 = lines[i + 1];
                var norad = ExtractNoradId(line1, line2);
                yield return new ParsedTle(UnknownName, line1, line2, norad);
                i += 2;
            }
            else if (i + 2 < lines.Count && lines[i + 1].StartsWith("1 ") && lines[i + 2].StartsWith("2 "))
            {
                var name = lines[i];
                var line1 = lines[i + 1];
                var line2 = lines[i + 2];
                var norad = ExtractNoradId(line1, line2);
                yield return new ParsedTle(name, line1, line2, norad);
                i += 3;
            }
            else
            {
                i++;
            }
        }
    }

    private static int ExtractNoradId(string line1, string line2)
    {
        var m = Line1Regex().Match(line1);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var id)) return id;
        m = Line2Regex().Match(line2);
        if (m.Success && int.TryParse(m.Groups[1].Value, out id)) return id;
        return 0;
    }

    [GeneratedRegex(@"^1\s*(\d{5})")]
    private static partial Regex Line1Regex();

    [GeneratedRegex(@"^2\s*(\d{5})")]
    private static partial Regex Line2Regex();
}