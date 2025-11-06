using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.Common.Linting
{
    [ExcludeFromCodeCoverage]
    public class RuleClassInfoDto
    {
        public Type RuleClassType { get; set; }

        public List<string> SupportedDataTypes { get; set; }

        public string RuleFullName { get; set; }

        public string RuleId { get; set; }

        public string RuleMnemo { get; set; }
    }
}
