using System;

namespace TeamTools.Common.Linting
{
    public interface IRuleFactory<TRule>
    {
        TRule MakeRule(Type ruleClass, ViolationCallbackEvent callback);
    }
}
