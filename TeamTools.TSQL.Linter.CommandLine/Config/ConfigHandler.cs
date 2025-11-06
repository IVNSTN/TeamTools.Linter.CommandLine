using System.Collections.Generic;
using System.Reflection;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Infrastructure;
using TeamTools.TSQL.Linter.CommandLine.Interfaces;

namespace TeamTools.TSQL.Linter.CommandLine.Config
{
    internal class ConfigHandler : IConfigHandler
    {
        private readonly string rootFolder;
        private readonly IAppConfigLoader reader;
        private readonly IFileSystemWrapper fileSystem;
        private readonly FileEnumeratorFactory factory;
        private readonly IAssemblyWrapper assembly;

        public ConfigHandler(IAssemblyWrapper assembly, IAppConfigLoader reader, IFileSystemWrapper fileSystem)
        {
            this.rootFolder = assembly.GetExecutingPath();
            this.assembly = assembly;
            this.reader = reader;
            this.fileSystem = fileSystem;
            this.factory = new FileEnumeratorFactory(fileSystem, rootFolder);
        }

        public IDictionary<string, PluginInfo> Plugins => reader.Plugins;

        public ICollection<string> IgnoredFolders => reader.IgnoredFolders;

        public ICollection<string> IgnoredExtensions => reader.IgnoredExtensions;

        public string GetAppVersion()
        {
            return assembly.GetVersion(Assembly.GetEntryAssembly());
        }

        public void LoadFromFile(string filePath)
        {
            reader.LoadFromFile(fileSystem.MakeAbsolutePath(rootFolder, filePath));
        }

        public IEnumerable<string> GetFilesToParse(CommandLineOptions opts)
        {
            var fileEnumerator = factory.Make(opts, IgnoredFolders, IgnoredExtensions);

            return fileEnumerator.EnumFiles();
        }
    }
}
