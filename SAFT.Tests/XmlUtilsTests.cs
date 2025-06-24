using System;
using System.IO;
using Xunit;
using SAFT.Lib.Utils;
using SAFT.Lib;

namespace SAFT.Tests
{
    public class XmlUtilsTests
    {
        [Fact]
        public void TestXmlUtils_SerializeToFile_UsesOutputDirectory()
        {
            var config = ConfigurationManager.Current;
            var testObj = new Header { CompanyName = "Test" };
            var fileName = "testheader.xml";
            var outputPath = Path.Combine(config.OutputDirectory, fileName);

            if (File.Exists(outputPath)) File.Delete(outputPath);
            XmlUtils.SerializeToFile(testObj, fileName);
            Assert.True(File.Exists(outputPath));
            File.Delete(outputPath);
        }
    }
} 