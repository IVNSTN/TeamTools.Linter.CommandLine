// original: https://github.com/tsqllint/tsqllint/blob/main/source/TSQLLint.Infrastructure/Plugins/AssemblyWrapper.cs
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace TeamTools.Common.Linting.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class AssemblyWrapper : IAssemblyWrapper
    {
        public Assembly Load(string path)
        {
            return Assembly.LoadFrom(path);
        }

        public Type[] GetExportedTypes(Assembly assembly)
        {
            return assembly.GetExportedTypes();
        }

        public string GetExecutingPath(Assembly assembly)
        {
            return Path.GetDirectoryName(assembly.Location);
        }

        public string GetExecutingPath()
        {
            return GetExecutingPath(System.Reflection.Assembly.GetExecutingAssembly());
        }

        public string GetVersion(Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }
    }
}
