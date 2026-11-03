using System.Text;
using System.Text.RegularExpressions;

namespace FileGuard.Core;

/// <summary>
/// Minimal glob matching for exclusion rules.
///   *   matches anything except '/'
///   **  matches anything including '/'
///   ?   matches one character except '/'
/// A pattern with no '/' (like "*.log") is also tried against just the file name.
/// </summary>
public static class Glob
{
    public static bool IsMatch(string pattern, string relativePath)
    {
        var path = relativePath.Replace('\\', '/');
        var regex = ToRegex(pattern.Replace('\\', '/'));

        if (Regex.IsMatch(path, regex)) return true;

        if (!pattern.Contains('/'))
            return Regex.IsMatch(Path.GetFileName(path), regex);

        return false;
    }

    private static string ToRegex(string pattern)
    {
        var sb = new StringBuilder("^");
        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            if (c == '*')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] == '*')
                {
                    sb.Append(".*");
                    i++;
                }
                else
                {
                    sb.Append("[^/]*");
                }
            }
            else if (c == '?')
            {
                sb.Append("[^/]");
            }
            else
            {
                sb.Append(Regex.Escape(c.ToString()));
            }
        }
        return sb.Append('$').ToString();
    }
}
