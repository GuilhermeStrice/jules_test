using System;
using Xunit;
using SAFT.Lib;
using SAFT.Lib.Utils;

namespace SAFT.Tests
{
    public class DefaultValueTests
    {
        [Fact]
        public void TestAddressStructure_UsesConfigDefaultCountry()
        {
            // Create a completely isolated configuration for this test
            var originalConfig = ConfigurationManager.Current;
            var testConfig = new SAFTConfiguration
            {
                SchemaPath = originalConfig.SchemaPath,
                OutputDirectory = originalConfig.OutputDirectory,
                StrictValidation = originalConfig.StrictValidation,
                DefaultCountryCode = "PT",
                DefaultCurrencyCode = originalConfig.DefaultCurrencyCode,
                DefaultFiscalYear = originalConfig.DefaultFiscalYear,
                CustomSettings = originalConfig.CustomSettings
            };
            
            // Set the test configuration
            ConfigurationManager.Set(testConfig);
            
            var address = new SAFT.Lib.AddressStructure { City = "Lisbon" };
            Assert.Equal("PT", address.Country);
            
            // Restore the original configuration
            ConfigurationManager.Set(originalConfig);
        }

        [Fact]
        public void TestHeader_UsesConfigDefaults()
        {
            // Ensure config is reset to default before test
            ConfigurationManager.Load();
            var header = new Header();
            Assert.Equal("PT", header.CompanyAddress.Country);
            Assert.Equal("EUR", header.CurrencyCode);
            // Reset to defaults after test
            ConfigurationManager.Load();
        }
    }
} 