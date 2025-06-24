using Xunit;
using SAFT.Lib.Utils;
using SAFT.Lib;

namespace SAFT.Tests
{
    public class DefaultValueTests
    {
        [Fact]
        public void TestHeader_UsesConfigDefaults()
        {
            var config = ConfigurationManager.Current;
            var header = Header.CreateDefault();
            Assert.Equal(config.DefaultCurrencyCode, header.CurrencyCode);
            Assert.Equal(config.DefaultCountryCode, header.CompanyAddress.Country);
        }

        [Fact]
        public void TestAddressStructure_UsesConfigDefaultCountry()
        {
            var config = ConfigurationManager.Current;
            var address = new AddressStructure();
            Assert.Equal(config.DefaultCountryCode, address.Country);
        }
    }
} 