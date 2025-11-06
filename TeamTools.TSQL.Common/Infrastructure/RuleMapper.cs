using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.Common.Linting.Infrastructure
{
    public static class RuleMapper
    {
        public static IEnumerable<RuleClassInfoDto> MapEnabledIdsToRules(IEnumerable<RuleClassInfoDto> allRules, IEnumerable<string> enabledRuleIds)
        {
            return allRules.Where(rule =>
                enabledRuleIds.Contains(rule.RuleFullName, StringComparer.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(rule.RuleId) && enabledRuleIds.Contains(rule.RuleId)));
        }
    }
}
