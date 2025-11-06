using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TeamTools.Common.Linting
{
    public abstract class BaseLintingConfig
    {
        // key - rule id, value - rule violation message
        public IDictionary<string, string> Rules { get; } = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // key - rule id, value - violation severity for VS and CI
        public IDictionary<string, Severity> RuleSeverity { get; } = new SortedDictionary<string, Severity>(StringComparer.OrdinalIgnoreCase);

        // key - datatype group name, value - list of file extensions
        public IDictionary<string, List<string>> SupportedFiles { get; } = new SortedDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        // key - filename mask, value - disabled rule id
        public IDictionary<Regex, List<string>> Whitelist { get; } = new Dictionary<Regex, List<string>>();

        public IEnumerable<string> SupportedFileTypes => SupportedFiles.Values.SelectMany(f => f).Distinct();
    }
}
