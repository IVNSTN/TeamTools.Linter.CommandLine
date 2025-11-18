using System;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.Common.Linting
{
    [ExcludeFromCodeCoverage]
    public sealed class RuleClassInfoDto
    {
        public Type RuleClassType { get; set; }

        public string[] SupportedDataTypes { get; set; }

        public string RuleFullName { get; set; }

        public string RuleId { get; set; }

        public string RuleMnemo { get; set; }
    }
}
