using System.Diagnostics.CodeAnalysis;

namespace TeamTools.Common.Linting
{
    // TODO : there is almost no difference with RuleViolationEventDto
    [ExcludeFromCodeCoverage]
    public class RuleViolation
    {
        public RuleViolation()
        { }

        public int Line { get; set; }

        public int Column { get; set; }

        public int? FragmentLength { get; set; }

        public string FileName { get; set; }

        public string RuleId { get; set; }

        public string Text { get; set; }

        public Severity ViolationSeverity { get; set; } = Severity.Error;

        public string SeverityName => SeverityConverter.ConvertToString(ViolationSeverity);

        public override string ToString()
        => $"{FileName}({Line},{Column}): {SeverityName} {RuleId} : {Text}";

        public RuleViolation Clone()
        {
            return new RuleViolation
            {
                Line = this.Line,
                Column = this.Column,
                FileName = this.FileName,
                RuleId = this.RuleId,
                Text = this.Text,
                FragmentLength = this.FragmentLength,
                ViolationSeverity = this.ViolationSeverity,
            };
        }
    }
}
