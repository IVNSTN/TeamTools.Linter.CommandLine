using System;
using System.Collections.Generic;
using System.IO;

namespace TeamTools.Common.Linting.Interfaces
{
    public interface ILinterHandler
    {
        void RunOnFileSource(string file, TextReader source, IReporter pluginRepoter);

        void RunOnFiles(IEnumerable<string> files, Action<string> reportVerbose, IReporter pluginRepoter);
    }
}
