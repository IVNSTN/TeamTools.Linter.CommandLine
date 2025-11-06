using System;

namespace TeamTools.Common.Linting.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class RuleGroupAttribute : Attribute
    {
        public RuleGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }

        public string GroupName { get; }
    }
}
