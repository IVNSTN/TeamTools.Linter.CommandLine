// original: https://github.com/tsqllint/tsqllint/blob/main/source/TSQLLint.Core/Interfaces/IAssemblyWrapper.cs
using System;
using System.Reflection;

namespace TeamTools.Common.Linting
{
    public interface IAssemblyWrapper
    {
        Assembly Load(string path);

        Type[] GetExportedTypes(Assembly assembly);

        string GetExecutingPath(Assembly assembly);

        string GetExecutingPath();

        string GetVersion(Assembly assembly);
    }
}
