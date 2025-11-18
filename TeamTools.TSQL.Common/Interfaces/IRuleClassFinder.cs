using System.Collections.Generic;

namespace TeamTools.Common.Linting
{
    public interface IRuleClassFinder
    {
        IEnumerable<RuleClassInfoDto> GetAvailableRuleClasses(IDictionary<string, string> enabledRuleIds);
    }
}
