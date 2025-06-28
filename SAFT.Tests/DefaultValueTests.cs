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
            ConfigurationManager.Current.DefaultCountryCode = "PT";
            var address = new SAFT.Lib.AddressStructure { City = "Lisbon" };
            Assert.Equal("PT", address.Country);
        }

        [Fact]
        public void TestHeader_UsesConfigDefaults()
        {
            // Ensure config is reset to default before test
            ConfigurationManager.Load();
            var header = new Header();
            Assert.Equal("PT", header.CompanyAddress.Country);
            Assert.Equal("EUR", header.CurrencyCode);
        }
    }
} 