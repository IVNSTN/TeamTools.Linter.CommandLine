using System;
using System.Collections.Generic;

namespace TeamTools.Common.Linting
{
    public static class SeverityConverter
    {
        private static readonly IDictionary<string, Severity> MapToEnum = new Dictionary<string, Severity>(StringComparer.OrdinalIgnoreCase)
        {
            { "Error", Severity.Error },
            { "Failure", Severity.Error },
            { "Warning", Severity.Warning },
            { "Warn", Severity.Warning },
            { "Info", Severity.Info },
            { "Hint", Severity.Info },
            { "Off", Severity.None },
        };

        private static readonly IDictionary<Severity, string> MapToString = new Dictionary<Severity, string>
        {
            { Severity.Error, "Error" },
            { Severity.Warning, "Warning" },
            { Severity.Info, "Info" },
            { Severity.None, "Off" },
        };

        public static Severity ConvertFromString(string severity)
        {
            if (string.IsNullOrEmpty(severity))
            {
                return Severity.None;
            }

            if (!MapToEnum.ContainsKey(severity))
            {
                throw new ArgumentOutOfRangeException(nameof(severity), $"'{severity}'");
            }

            return MapToEnum[severity];
        }

        public static string ConvertToString(Severity severity) => MapToString[severity];
    }
}
