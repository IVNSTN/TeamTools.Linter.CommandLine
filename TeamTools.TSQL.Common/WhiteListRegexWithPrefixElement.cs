using System;

namespace TeamTools.Common.Linting
{
    public sealed class WhiteListRegexWithPrefixElement : WhiteListRegexElement
    {
        private readonly string prefix;

        public WhiteListRegexWithPrefixElement(string prefix, string pattern) : base(pattern)
        {
            this.prefix = prefix;
        }

        public override bool IsMatch(string filename)
        {
            return filename.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && base.IsMatch(filename);
        }
    }
}
