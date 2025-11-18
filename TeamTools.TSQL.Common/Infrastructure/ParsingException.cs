using System;

namespace TeamTools.Common.Linting
{
    public class ParsingException : Exception
    {
        public ParsingException(string msg, int line, int col) : base(msg)
        {
            this.Line = line;
            this.Col = col;
        }

        public int Line { get; }

        public int Col { get; }
    }
}
