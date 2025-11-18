using System;

namespace TeamTools.Common.Linting
{
    public abstract class WhiteListElement
    {
        private readonly string pattern;

        protected WhiteListElement(string pattern)
        {
            this.pattern = pattern;
        }

        public virtual bool IsMatch(string filename)
        {
            return string.Equals(filename, pattern, StringComparison.OrdinalIgnoreCase);
        }
    }
}
