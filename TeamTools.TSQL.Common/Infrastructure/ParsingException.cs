using System;

namespace TeamTools.Common.Linting
{
    public class ParsingException : Exception
    {
        private readonly int line;
        private readonly int col;

        public ParsingException(string msg, int line, int col) : base(msg)
        {
            this.line = line;
            this.col = col;
        }

        public int Line => line;

        public int Col => col;
    }
}
