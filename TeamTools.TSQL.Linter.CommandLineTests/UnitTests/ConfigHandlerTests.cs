using NUnit.Framework;
using TeamTools.Common.Linting;
using TeamTools.Common.Linting.Infrastructure;
using TeamTools.TSQL.Linter.CommandLine.Config;

namespace TeamTools.TSQL.Linter.CommandLineTests
{
    [Category("Linter.ConsoleExe")]
    public class ConfigHandlerTests
    {
        [Test]
        public void TestConfigHandlerParsesConfigCorrectly()
        {
            var fs = new FileSystemWrapper();
            var asm = new AssemblyWrapper();
            var cfg = new ConfigHandler(asm, new AppConfigLoader(fs, asm), fs);

            cfg.LoadFromFile(@".\DefaultConfig.json");

            Assert.Multiple(() =>
            {
                Assert.That(cfg.IgnoredExtensions, Is.Not.Empty, "ignored extensions not loaded");
                Assert.That(cfg.IgnoredFolders, Is.Not.Empty, "ignored folers not loaded");
                Assert.That(cfg.Plugins, Has.Count.EqualTo(3), "plugins not loaded");
            });
        }
    }
}
