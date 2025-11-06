// original: https://github.com/tsqllint/tsqllint/blob/main/source/TSQLLint.Infrastructure/Reporters/NonBlockingConsole.cs
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class NonBlockingConsole
    {
        static NonBlockingConsole()
        {
            // TODO : rethink
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    Console.WriteLine(MessageQueue.Take());
                }
            }).Start();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    Console.Error.WriteLine(ErrorQueue.Take());
                }
            }).Start();
        }

        public static BlockingCollection<string> MessageQueue { get; } = new BlockingCollection<string>();

        public static BlockingCollection<string> ErrorQueue { get; } = new BlockingCollection<string>();

        public static void WriteLine(string value)
        {
            MessageQueue.Add(value);
        }

        public static void WriteError(string value)
        {
            ErrorQueue.Add(value);
        }
    }
}
