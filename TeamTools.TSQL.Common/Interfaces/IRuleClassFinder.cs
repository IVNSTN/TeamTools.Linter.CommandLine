using System.Collections.Generic;

namespace TeamTools.Common.Linting
{
    public interface IRuleClassFinder
    {
        IEnumerable<RuleClassInfoDto> GetAvailableRuleClasses(IEnumerable<string> enabledRuleIds);
    }
}
