using System;
using System.IO;

namespace SAFT.Tests
{
    public static class Helpers
    {
        public static string GetSchemaPath()
        {
            var solutionDir = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(solutionDir, "SAFT.sln")))
            {
                solutionDir = Directory.GetParent(solutionDir)?.FullName;
                if (solutionDir == null)
                    throw new FileNotFoundException("Could not find SAFT.sln file");
            }
            var schemaPath = Path.Combine(solutionDir, "schema1_04_fixed.xsd");
            EnsureSchemaInOutputDirectory(schemaPath);
            return schemaPath;
        }

        private static void EnsureSchemaInOutputDirectory(string schemaPath)
        {
            var outputDir = Directory.GetCurrentDirectory();
            var outputSchemaPath = Path.Combine(outputDir, "schema1_04_fixed.xsd");
            if (!File.Exists(outputSchemaPath) && File.Exists(schemaPath))
            {
                File.Copy(schemaPath, outputSchemaPath);
            }
        }

        public static string GetSaftPath()
        {
            var solutionDir = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(solutionDir, "SAFT.sln")))
            {
                solutionDir = Directory.GetParent(solutionDir)?.FullName;
                if (solutionDir == null)
                    throw new FileNotFoundException("Could not find SAFT.sln file");
            }
            return Path.Combine(solutionDir, "valid_saft.xml");
        }
    }
} 