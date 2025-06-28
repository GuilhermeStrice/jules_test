using System;
using System.IO;
using Xunit;
using SAFT.Lib.Utils;

namespace SAFT.Tests
{
    public class ConfigurationManagerTests
    {
        [Fact]
        public void TestConfigurationManager_LoadsDefaults()
        {
            // Remove config file if exists
            if (File.Exists("saftconfig.json")) File.Delete("saftconfig.json");
            ConfigurationManager.Load();
            var config = ConfigurationManager.Current;
            Assert.Equal("schema1_04.xsd", config.SchemaPath);
            Assert.Equal("./output", config.OutputDirectory);
        }

        [Fact]
        public void TestConfigurationManager_OverrideAndSave()
        {
            // Ensure clean state before test
            ConfigurationManager.Load();
            if (File.Exists("testconfig.json")) File.Delete("testconfig.json");
            var customConfig = new SAFTConfiguration
            {
                SchemaPath = "custom.xsd",
                OutputDirectory = "./custom_output",
                DefaultCountryCode = "ES"
            };
            ConfigurationManager.Set(customConfig);
            ConfigurationManager.Save("testconfig.json");
            Assert.True(File.Exists("testconfig.json"));
            var json = File.ReadAllText("testconfig.json");
            
            // Debug output
            Console.WriteLine($"[DEBUG] Generated JSON: {json}");
            
            Assert.Contains("custom.xsd", json);
            Assert.Contains("custom_output", json);
            Assert.Contains("ES", json);
            File.Delete("testconfig.json");
            // Reset config to default after test
            ConfigurationManager.Load();
        }
    }
} 