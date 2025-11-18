using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.CommandLine.Interfaces
{
    public interface IFileEnumerator
    {
        IEnumerable<string> EnumFiles();
    }
}
