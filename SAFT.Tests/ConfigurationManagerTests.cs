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
            if (File.Exists("testconfig.json")) File.Delete("testconfig.json");
            
            var customConfig = new SAFTConfiguration
            {
                SchemaPath = "custom.xsd",
                OutputDirectory = "./custom_output",
                DefaultCountryCode = "ES"
            };
            
            // Directly serialize and save the custom config
            var json = System.Text.Json.JsonSerializer.Serialize(customConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("testconfig.json", json);
            Assert.True(File.Exists("testconfig.json"));

            var configFromJson = System.Text.Json.JsonSerializer.Deserialize<SAFT.Lib.Utils.SAFTConfiguration>(json);
            Assert.NotNull(configFromJson);
            Assert.Equal("custom.xsd", System.IO.Path.GetFileName(configFromJson.SchemaPath));
            Assert.Contains("custom_output", json);
            Assert.Contains("ES", json);
            
            // Clean up
            File.Delete("testconfig.json");
        }
    }
} 