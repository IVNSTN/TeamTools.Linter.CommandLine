using System;
using System.Collections.Generic;

namespace TeamTools.Common.Linting
{
    public interface IRuleCollectionHandler<T, TP>
    {
        IDictionary<string, List<RuleInstance<T>>> Rules { get; }

        void MakeRules();

        void ApplyRulesTo(
            ILintingContext context,
            Action<T, TP> processor);

        int RuleCount();
    }
}
