using NUnit.Framework;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLineTests.UnitTests
{
    [TestOf(typeof(SeverityConverter))]
    [Category("Linter.ConsoleExe")]
    public class SeverityConverterTests
    {
        [Test]
        public void Test_SeverityConverter_ConvertsFromString()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SeverityConverter.ConvertFromString("error"), Is.EqualTo(Severity.Error));
                Assert.That(SeverityConverter.ConvertFromString("WARNING"), Is.EqualTo(Severity.Warning));
                Assert.That(SeverityConverter.ConvertFromString("info"), Is.EqualTo(Severity.Info));
                Assert.That(SeverityConverter.ConvertFromString("Hint"), Is.EqualTo(Severity.Info));
                Assert.That(SeverityConverter.ConvertFromString("off"), Is.EqualTo(Severity.None));
            });
        }

        [Test]
        public void Test_SeverityConverter_ReturnsNoneForEmptyString()
        {
            Assert.That(SeverityConverter.ConvertFromString(""), Is.EqualTo(Severity.None));
            Assert.That(SeverityConverter.ConvertFromString(null), Is.EqualTo(Severity.None));
        }

        [Test]
        public void Test_SeverityConverter_FailsForUnknownString()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => SeverityConverter.ConvertFromString("dummy"));
        }

        [Test]
        public void Test_SeverityConverter_ConvertsToString()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SeverityConverter.ConvertToString(Severity.Error), Is.EqualTo("Error"));
                Assert.That(SeverityConverter.ConvertToString(Severity.Warning), Is.EqualTo("Warning"));
                Assert.That(SeverityConverter.ConvertToString(Severity.Info), Is.EqualTo("Info"));
                Assert.That(SeverityConverter.ConvertToString(Severity.None), Is.EqualTo("Off"));
            });
        }
    }
}
