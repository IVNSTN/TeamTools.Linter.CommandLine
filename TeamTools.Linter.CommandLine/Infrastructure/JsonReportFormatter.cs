using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    internal class JsonReportFormatter : IReportOutputFormatter
    {
        private readonly string basePath;

        public JsonReportFormatter(string basePath)
        {
            this.basePath = basePath;
        }

        public void Write(IEnumerable<RuleViolation> violations, TextWriter output)
        {
            var fileIssues = new Dictionary<string, List<IssueDescr>>(StringComparer.OrdinalIgnoreCase);

            foreach (var violation in violations)
            {
                string filePathRelative = violation.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (!string.IsNullOrEmpty(basePath))
                {
                    filePathRelative = Path.GetRelativePath(basePath, filePathRelative);
                }

                if (!fileIssues.TryGetValue(filePathRelative, out var registeredFileIssues))
                {
                    registeredFileIssues = new List<IssueDescr>();
                    fileIssues.Add(filePathRelative, registeredFileIssues);
                }

                registeredFileIssues.Add(new IssueDescr
                {
                    Line = violation.Line,
                    Col = violation.Column,
                    Reason = violation.Text,
                    Rule = violation.RuleId,
                    Severity = "MAJOR", // TODO : take from config
                    Category = "CODE_SMELL",
                    Evidence = "",
                });
            }

            var report = new Dictionary<string, TopLevelDescr>
            {
                { "LINT", new TopLevelDescr { Language = "TSQL" } },
            };
            report["LINT"].Files.AddRange(
                fileIssues
                .Select(issue => new FileIssues { Name = issue.Key, Issues = issue.Value })
                .OrderBy(issue => issue.Name));

            output.WriteLine(JsonSerializer.Serialize(report));
        }

        private sealed class IssueDescr
        {
            [JsonPropertyName("line")]
            public int Line { get; set; }

            [JsonPropertyName("col")]
            public int Col { get; set; }

            [JsonPropertyName("reason")]
            public string Reason { get; set; }

            [JsonPropertyName("evidence")]
            public string Evidence { get; set; }

            [JsonPropertyName("category")]
            public string Category { get; set; }

            [JsonPropertyName("severity")]
            public string Severity { get; set; }

            [JsonPropertyName("rule")]
            public string Rule { get; set; }
        }

        private sealed class FileIssues
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("issues")]
            public List<IssueDescr> Issues { get; set; } = new List<IssueDescr>();
        }

        private sealed class TopLevelDescr
        {
            [JsonPropertyName("language")]
            public string Language { get; set; }

            [JsonPropertyName("files")]
            public List<FileIssues> Files { get; set; } = new List<FileIssues>();
        }
    }
}
