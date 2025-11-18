// original: https://github.com/tsqllint/tsqllint/blob/main/source/TSQLLint.Infrastructure/Reporters/ConsoleReporter.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TeamTools.Common.Linting;
using TeamTools.Linter.CommandLine.Properties;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class ConsoleReporter : IReporter, IReporterDecorator, IDisposable
    {
        protected const string ErrorSeverity = "error";
        private readonly ConcurrentBag<RuleViolation> ruleViolations = new ConcurrentBag<RuleViolation>();
        private readonly NonBlockingConsole nbcs;
        private int errorCount;

        public ConsoleReporter() : this(default)
        { }

        public ConsoleReporter(string basePath) : this(basePath, Encoding.UTF8)
        { }

        private ConsoleReporter(string basePath, Encoding outputEncoding)
        {
            Console.OutputEncoding = outputEncoding;
            BasePath = basePath;
            this.nbcs = new NonBlockingConsole();

            var printMessages = Task.Run(() =>
            {
                while (!nbcs.MessageQueue.IsCompleted)
                {
                    if (nbcs.MessageQueue.TryTake(out string msg))
                    {
                        Console.WriteLine(msg);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            });

            var printErrors = Task.Run(() =>
            {
                while (!nbcs.ErrorQueue.IsCompleted)
                {
                    if (nbcs.ErrorQueue.TryTake(out string msg))
                    {
                        Console.Error.WriteLine(msg);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            });

            CompleteReporting = Task.WhenAll(printMessages, printErrors);
        }

        public bool ReporterMuted { get; set; }

        public bool ShouldCollectViolations { get; set; } = false;

        // TODO : Extract something like ViolationWriter?
        public bool ShouldReporViolationsAsFailures { get; set; } = false;

        public string BasePath { get; set; }

        public IEnumerable<RuleViolation> Violations => ruleViolations.ToArray();

        public Task CompleteReporting { get; }

        public int ViolationCount => errorCount;

        public void ClearViolations()
        {
            ruleViolations.Clear();
        }

        [ExcludeFromCodeCoverage]
        public virtual void Report(string message)
        {
            nbcs.WriteLine(message);
        }

        [ExcludeFromCodeCoverage]
        public virtual void ReportFailure(string error)
        {
            nbcs.WriteError(error);
        }

        public virtual void ReportResults()
        {
            Report(string.Format(Strings.AppMessages_total_violations, errorCount.ToString()));
        }

        public void ReportViolation(RuleViolation violation)
        {
            if (ShouldCollectViolations)
            {
                ruleViolations.Add(violation);
            }

            // If `--fix` is turned on sometimes the program runs a couple of times
            // caused by fixing another rule violation. This condition prevents double or tripple counting.
            if (!ReporterMuted)
            {
                errorCount++;
            }

            // TODO : no, this path reduction has to occure elsewhere before getting to reporter
            string msg = violation.ToString();
            if (!string.IsNullOrEmpty(BasePath) && msg.StartsWith(BasePath, System.StringComparison.OrdinalIgnoreCase))
            {
                msg = msg.Substring(BasePath.Length).TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            if (ShouldReporViolationsAsFailures)
            {
                ReportFailure(msg);
            }
            else
            {
                Report(msg);
            }
        }

        public void Finish()
        {
            nbcs.ErrorQueue.CompleteAdding();
            nbcs.MessageQueue.CompleteAdding();
        }

        public void Dispose()
        {
            nbcs.Dispose();
        }
    }
}
