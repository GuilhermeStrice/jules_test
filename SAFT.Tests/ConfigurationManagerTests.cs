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
            Assert.Contains("custom.xsd", json);
            Assert.Contains("custom_output", json);
            File.Delete("testconfig.json");
        }
    }
} 