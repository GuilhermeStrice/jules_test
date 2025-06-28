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
            // Use a unique temporary directory for this test
            var tempDir = Path.Combine(Path.GetTempPath(), "saft_test_output_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);
            var fileName = "testheader.xml";
            var outputPath = Path.Combine(tempDir, fileName);

            var testObj = new Header { CompanyName = "Test" };

            // Debug output for output directory
            Console.WriteLine($"[DEBUG] Temp OutputDirectory: {tempDir}");
            Console.WriteLine($"[DEBUG] Resolved output path: {outputPath}");

            // Clean up before test
            if (File.Exists(outputPath)) File.Delete(outputPath);

            XmlUtils.SerializeToFile(testObj, outputPath);

            // Debug output
            Console.WriteLine($"[DEBUG] Expected output path: {outputPath}");
            Console.WriteLine($"[DEBUG] File exists after serialization: {File.Exists(outputPath)}");
            Console.WriteLine($"[DEBUG] Current Directory: {Directory.GetCurrentDirectory()}");

            Assert.True(File.Exists(outputPath));

            // Clean up after test
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            if (Directory.Exists(tempDir) && Directory.GetFiles(tempDir).Length == 0)
                Directory.Delete(tempDir);
        }
    }
} 