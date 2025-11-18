using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace TeamTools.Common.Linting
{
    internal class GitProcessRunner
    {
        private const string GitExe = "git.exe";
        private readonly ITextOutputPort outputPort;
        private readonly string workingFolder;

        public GitProcessRunner(ITextOutputPort output, string workingFolder)
        {
            this.outputPort = output;
            this.workingFolder = workingFolder;
        }

        public void ExecuteCmd(string cmd)
        {
            // replacing quotes is a ProcessStartInfo thing
            // console execution without replacing is just fine
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingFolder,
                FileName = GitExe,
                Arguments = cmd.Replace("'", "\""),
            };

            Debug.WriteLine(GitExe + " " + startInfo.Arguments);

            Process process = new Process { StartInfo = startInfo };
            RunProcessAndGrabOutput(process);
            process.WaitForExit();
        }

        private static void SetEvent(AutoResetEvent waitHandle)
        {
            try
            {
                waitHandle.Set();
            }
            catch (Exception)
            {
                // FIXME : handle exception properly; and try to fix the cause
            }
        }

        private void RunProcessAndGrabOutput(Process process)
        {
            GrabProcessOutput(process, outputPort);
        }

        private void GrabProcessOutput(Process process, ITextOutputPort output)
        {
            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data is null)
                    {
                        SetEvent(outputWaitHandle);
                    }
                    else
                    {
                        output.WriteLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data is null)
                    {
                        SetEvent(errorWaitHandle);
                    }
                    else
                    {
                        output.WriteLine(e.Data);
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                const int timeout = 5000;

                if (process.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout))
                {
                    // TODO : Process completed. Check process.ExitCode here.
                }
                else
                {
                    // TODO : Timed out. Handle properly.
                }
            }

            process.WaitForExit(300);
        }
    }
}
