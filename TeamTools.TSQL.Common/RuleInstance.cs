using System.Diagnostics.CodeAnalysis;

namespace TeamTools.Common.Linting
{
    [ExcludeFromCodeCoverage]
    public class RuleInstance<T>
    {
        public string RuleFullName { get; set; }

        public T Rule { get; set; }
    }
}
