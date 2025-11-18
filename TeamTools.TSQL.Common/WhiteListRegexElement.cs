using System.Text;
using System.Text.RegularExpressions;

namespace TeamTools.Common.Linting
{
    public abstract class WhiteListRegexElement : WhiteListElement
    {
        private readonly Regex regex;
        private readonly int minLength;

        protected WhiteListRegexElement(string pattern) : base(pattern)
        {
            this.regex = new Regex(MakePatternCaseInsensitive(SanitizePattern(pattern)), RegexOptions.Compiled);
            // '*' means zero or more occurances
            // removing them to find minimal filename length to match given pattern
            this.minLength = pattern.Replace("*", "").Length;
        }

        public override bool IsMatch(string filename)
        {
            // If filename is not long enough to match given pattern
            // then no need of running regexp matching. Length comparison is much faster.
            return filename.Length >= minLength
                && regex.IsMatch(filename);
        }

        private static string SanitizePattern(string pattern)
        {
            var sanitizedPattern = Regex.Escape(pattern.Replace("*", "#many#").Replace("?", "#single#"));
            sanitizedPattern = sanitizedPattern.Replace("\\#single\\#", ".?").Replace("\\#many\\#", ".*");
            return $"^{sanitizedPattern}$";
        }

        private static string MakePatternCaseInsensitive(string pattern)
        {
            int n = pattern.Length;
            var lower = pattern.ToLowerInvariant();
            var upper = pattern.ToUpperInvariant();

            var result = new StringBuilder();

            for (int i = 0; i < n; i++)
            {
                if (lower[i] == upper[i])
                {
                    result.Append(pattern[i]);
                }
                else
                {
                    result.Append('[').Append(lower[i]).Append(upper[i]).Append(']');
                }
            }

            return result.ToString();
        }
    }
}
