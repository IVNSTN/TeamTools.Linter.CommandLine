using System;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.Common.Linting
{
    public delegate void ViolationCallbackEvent(object sender, RuleViolationEventDto dto);

    // TODO : feels line RuleViolation class is enough for any purpose
    [ExcludeFromCodeCoverage]
    public class RuleViolationEventDto : EventArgs
    {
        public RuleViolationEventDto()
        { }

        public string FileName { get; set; }

        public string RuleId { get; set; }

        public string ErrorDetails { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public int? FragmentLength { get; set; }

        public Severity ViolationSeverity { get; set; } = Severity.Error;
    }
}
