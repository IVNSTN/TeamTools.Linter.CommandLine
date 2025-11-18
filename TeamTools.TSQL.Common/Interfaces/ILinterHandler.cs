using System;
using System.Collections.Generic;
using System.IO;

namespace TeamTools.Common.Linting.Interfaces
{
    public interface ILinterHandler
    {
        // For inline linting in VSIX
        void RunOnFileSource(string file, TextReader source, IReporter pluginRepoter);

        // For linting given list of files on disk
        void RunOnFiles(IEnumerable<string> files, Action<string> reportVerbose, IReporter pluginRepoter);
    }
}
