// original: https://github.com/tsqllint/tsqllint/blob/main/source/TSQLLint.Infrastructure/Reporters/NonBlockingConsole.cs
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class NonBlockingConsole : IDisposable
    {
        private const int BufferSize = 8192;
        private readonly StreamWriter consoleWriter;
        private readonly StreamWriter errorWriter;

        public NonBlockingConsole()
        {
            consoleWriter = new StreamWriter(Console.OpenStandardOutput(BufferSize), bufferSize: BufferSize);
            consoleWriter.AutoFlush = true; // FIXME: sometimes nothing is reported to console if `false` whereas violations are detected
            Console.SetOut(consoleWriter);

            errorWriter = new StreamWriter(Console.OpenStandardError(BufferSize), bufferSize: BufferSize);
            consoleWriter.AutoFlush = true; // FIXME: sometimes nothing is reported to console if `false` whereas violations are detected
            Console.SetError(errorWriter);
        }

        public BlockingCollection<string> MessageQueue { get; } = new BlockingCollection<string>(new ConcurrentQueue<string>());

        public BlockingCollection<string> ErrorQueue { get; } = new BlockingCollection<string>(new ConcurrentQueue<string>());

        public void WriteLine(string value)
        {
            MessageQueue.Add(value);
        }

        public void WriteError(string value)
        {
            ErrorQueue.Add(value);
        }

        public void Dispose()
        {
            consoleWriter.Flush();
            consoleWriter.Close();
            consoleWriter.Dispose();

            errorWriter.Flush();
            errorWriter.Close();
            errorWriter.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
