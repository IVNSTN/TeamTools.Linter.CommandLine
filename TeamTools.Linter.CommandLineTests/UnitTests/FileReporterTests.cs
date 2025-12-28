using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Infrastructure;

namespace TeamTools.TSQL.Linter.CommandLineTests
{
    [Category("Linter.ConsoleExe")]
    public class FileReporterTests
    {
        [Test]
        public void TestReportResultsSendsToStream()
        {
            var writer = new StringWriter();
            var formatter = new MockReportFormatter();
            var reporter = new MockFileReporter(writer, new ConsoleReporter(), formatter);

            reporter.ReportViolation(new RuleViolation
            {
                Text = "issue1",
            });

            reporter.ReportViolation(new RuleViolation
            {
                Text = "issue2",
            });

            reporter.ReportResults();

            // ToList, ToArray returns elements in reversed order
            Assert.That(writer.ToString(), Is.EqualTo(string.Join(Environment.NewLine, "error: issue2", "error: issue1", "")));
        }

        [Test]
        public void TestJsonReportFormatterOutput()
        {
#if Windows
            const string basePath = @"c:\";
#else
            const string basePath = @"/home/";
#endif
            string expectedOutput = @"{'LINT':{'language':'TSQL','files':[{'name':'filename.txt','issues':[{'line':2,'col':3,'reason':'Failure','evidence':'','category':'CODE_SMELL','severity':'MAJOR','rule':'RULEID'}]}]}}"
                .Replace('\'', '"');
            var formatter = new JsonReportFormatter(Path.Combine(basePath, "test"));
            var output = new StringWriter();
            var issues = new List<RuleViolation>();
            issues.Add(new RuleViolation
            {
                Line = 2,
                Column = 3,
                FileName = Path.Combine(basePath, "test", "filename.txt"),
                RuleId = "RULEID",
                Text = "Failure",
            });

            formatter.Write(issues, output);

            Assert.That(output.ToString().Trim('\r', '\n'), Is.EqualTo(expectedOutput));
        }

        private class MockFileReporter : FileReporter
        {
            public MockFileReporter(TextWriter writer, ConsoleReporter consoleReporter, IReportOutputFormatter formatter) : base(writer, consoleReporter, formatter)
            {
            }

            public override void Report(string msg)
            {
                // do nothing
            }
        }

        private class MockReportFormatter : IReportOutputFormatter
        {
            public void Write(IEnumerable<RuleViolation> violations, TextWriter output)
            {
                foreach (var v in violations)
                {
                    output.WriteLine("error: " + v.Text);
                }
            }
        }
    }
}
