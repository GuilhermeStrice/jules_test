using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WireGuardManager.Web.Services;
using Xunit;

namespace WireGuardManager.Tests
{
    // Helper class to mock ExecuteCommandAsync
    public class TestableWireGuardService : WireGuardService
    {
        private readonly Func<string, string, Task<(string Output, string Error, int ExitCode)>> _mockExecuteCommandAsync;

        public TestableWireGuardService(Func<string, string, Task<(string Output, string Error, int ExitCode)>> mockExecuteCommandAsync)
        {
            _mockExecuteCommandAsync = mockExecuteCommandAsync;
        }

        protected override async Task<(string Output, string Error, int ExitCode)> ExecuteCommandAsync(string command, string arguments)
        {
            if (_mockExecuteCommandAsync != null)
            {
                return await _mockExecuteCommandAsync(command, arguments);
            }
            // Fallback to base if no mock is provided, though for tests we usually want the mock.
            // Or throw an exception if a mock was expected.
            return await base.ExecuteCommandAsync(command, arguments);
        }

        // Expose a way to set specific mock responses per command if needed, or keep it simple with one func.
        // For more complex scenarios, a dictionary of command -> response could be used.
    }

    public class WireGuardServiceTests
    {
        [Fact]
        public async Task GeneratePeerConfigAsync_ValidInputs_ReturnsCorrectConfigString()
        {
            // Arrange
            var service = new WireGuardService(); // No command execution, can use base class
            var peerPrivateKey = "TestPeerPrivateKey=";
            var peerPresharedKey = "TestPeerPresharedKey=";
            var peerDnsServers = new List<string> { "1.1.1.1", "8.8.8.8" };
            var interfaceAddress = "10.0.0.2/32";
            var serverPublicKey = "TestServerPublicKey=";
            var serverEndpoint = "server.example.com:51820";
            var serverAllowedIPs = "0.0.0.0/0, ::/0";

            var expectedConfig =
                $"[Interface]{Environment.NewLine}" +
                $"PrivateKey = {peerPrivateKey}{Environment.NewLine}" +
                $"Address = {interfaceAddress}{Environment.NewLine}" +
                $"DNS = {string.Join(",", peerDnsServers)}{Environment.NewLine}" +
                $"{Environment.NewLine}" +
                $"[Peer]{Environment.NewLine}" +
                $"PublicKey = {serverPublicKey}{Environment.NewLine}" +
                $"PresharedKey = {peerPresharedKey}{Environment.NewLine}" +
                $"AllowedIPs = {serverAllowedIPs}{Environment.NewLine}" +
                $"Endpoint = {serverEndpoint}{Environment.NewLine}" +
                $"PersistentKeepalive = 25{Environment.NewLine}";

            // Act
            var (peerConfig, errorMessage) = await service.GeneratePeerConfigAsync(
                peerPrivateKey, peerPresharedKey, peerDnsServers, interfaceAddress,
                serverPublicKey, serverEndpoint, serverAllowedIPs);

            // Assert
            Assert.True(string.IsNullOrEmpty(errorMessage), $"Error message should be empty, but was: {errorMessage}");
            Assert.NotNull(peerConfig);
            Assert.Equal(expectedConfig.Trim(), peerConfig.Trim()); // Trim to handle potential trailing newlines
        }

        [Fact]
        public async Task GeneratePeerConfigAsync_MinimalValidInputs_ReturnsCorrectConfigString()
        {
            // Arrange
            var service = new WireGuardService();
            var peerPrivateKey = "AnotherPeerPrivateKey=";
            // No PresharedKey, No DNS
            var interfaceAddress = "192.168.2.5/32";
            var serverPublicKey = "AnotherServerPublicKey=";
            var serverEndpoint = "test.domain.org:12345";
            var serverAllowedIPs = "10.0.0.0/24";

            var expectedConfig =
                $"[Interface]{Environment.NewLine}" +
                $"PrivateKey = {peerPrivateKey}{Environment.NewLine}" +
                $"Address = {interfaceAddress}{Environment.NewLine}" +
                $"{Environment.NewLine}" + // DNS line is omitted
                $"[Peer]{Environment.NewLine}" +
                $"PublicKey = {serverPublicKey}{Environment.NewLine}" +
                // PresharedKey line is omitted
                $"AllowedIPs = {serverAllowedIPs}{Environment.NewLine}" +
                $"Endpoint = {serverEndpoint}{Environment.NewLine}" +
                $"PersistentKeepalive = 25{Environment.NewLine}";

            // Act
            var (peerConfig, errorMessage) = await service.GeneratePeerConfigAsync(
                peerPrivateKey, null, null, interfaceAddress, // Pass null for optional params
                serverPublicKey, serverEndpoint, serverAllowedIPs);

            // Assert
            Assert.True(string.IsNullOrEmpty(errorMessage), $"Error message should be empty, but was: {errorMessage}");
            Assert.NotNull(peerConfig);
            Assert.Equal(expectedConfig.Trim(), peerConfig.Trim());
        }

        [Fact]
        public async Task GeneratePeerConfigAsync_MissingRequiredInput_ReturnsError()
        {
            // Arrange
            var service = new WireGuardService();

            // Act: Missing serverPublicKey
            var (peerConfig, errorMessage) = await service.GeneratePeerConfigAsync(
                "PrivateKey", null, null, "10.0.0.1/32",
                null, "endpoint", "0.0.0.0/0");

            // Assert
            Assert.Null(peerConfig);
            Assert.False(string.IsNullOrEmpty(errorMessage));
            Assert.Contains("Server public key is required", errorMessage);
        }

        [Fact]
        public async Task GenerateKeypairAsync_Success_ReturnsKeys()
        {
            // Arrange
            var expectedPrivateKey = "MockPrivateKey=";
            var expectedPublicKey = "MockPublicKey=";

            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg" && arguments == "genkey")
                {
                    return Task.FromResult((expectedPrivateKey, string.Empty, 0));
                }
                // Note: The actual implementation of GenerateKeypairAsync pipes the private key to 'wg pubkey'.
                // This mock setup is simplified; a more robust mock might inspect the input to 'wg pubkey'.
                // However, for this test, we assume 'wg pubkey' is called correctly after 'wg genkey'.
                // The service's internal ProcessStartInfo for 'wg pubkey' will handle stdin/stdout.
                // Our ExecuteCommandAsync is not directly called for 'wg pubkey' in the current service design,
                // as it uses a new Process. This highlights a limitation of the current mocking strategy for this specific method.
                // For this test, we'll assume the internal 'wg pubkey' call works if 'wg genkey' was successful.
                // A more complete test would require refactoring GenerateKeypairAsync to use ExecuteCommandAsync for pubkey generation too.
                // OR, we can test the 'wg pubkey' part separately if it were its own method.

                // Given the current GenerateKeypairAsync structure, we can't easily mock the 'wg pubkey' part
                // without refactoring how it calls 'wg pubkey' (e.g. to use ExecuteCommandAsync).
                // So, this test will primarily verify 'wg genkey' and that *if* 'wg pubkey' were mocked by this mechanism,
                // it would proceed. The current structure of GenerateKeypairAsync bypasses our ExecuteCommandAsync mock for pubkey.
                //
                // Let's adjust the test to reflect what we *can* test:
                // We can mock genkey. The pubkey part is harder with the current TestableWireGuardService.
                // For now, let's assume if genkey is successful, the internal pubkey call (which is not using our mocked ExecuteCommandAsync)
                // would also be successful if the `wg` tool is functional.
                // This test is therefore more of an integration test for the `wg pubkey` part.
                //
                // To make it a pure unit test, GenerateKeypairAsync would need refactoring.
                // For this exercise, we will proceed acknowledging this limitation.
                // The test will pass if genkey is mocked and the actual 'wg pubkey' command (if 'wg' is installed) works.
                // If 'wg' is not installed in the test environment, the 'wg pubkey' call will fail.

                // To actually mock the 'wg pubkey' part effectively with the current helper,
                // GenerateKeypairAsync would need to be changed to use ExecuteCommandAsync for the pubkey part.
                // Since it doesn't, this mock for 'wg pubkey' won't be hit by the current implementation.
                if (command == "wg" && arguments == "pubkey")
                {
                     // This part of the mock won't be hit by the current GenerateKeypairAsync implementation.
                    return Task.FromResult((expectedPublicKey, string.Empty, 0));
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (publicKey, privateKey, errorMessage) = await service.GenerateKeypairAsync();

            // Assert
            // Due to the `wg pubkey` execution not using the overridden ExecuteCommandAsync,
            // we can only reliably assert the private key from the mocked `wg genkey`.
            // The public key will be the result of the actual `wg pubkey` command run on the private key.
            Assert.True(string.IsNullOrEmpty(errorMessage), $"Error: {errorMessage}");
            Assert.Equal(expectedPrivateKey, privateKey);
            Assert.False(string.IsNullOrEmpty(publicKey), "Public key should be generated.");
            // We cannot assert expectedPublicKey reliably unless 'wg' is installed and behaves predictably.
        }


        [Fact]
        public async Task GenerateKeypairAsync_GenkeyFails_ReturnsError()
        {
            // Arrange
            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg" && arguments == "genkey")
                {
                    return Task.FromResult((string.Empty, "genkey failed", 1));
                }
                return Task.FromResult((string.Empty, string.Empty, 0)); // Should not be reached
            });

            // Act
            var (publicKey, privateKey, errorMessage) = await service.GenerateKeypairAsync();

            // Assert
            Assert.False(string.IsNullOrEmpty(errorMessage));
            Assert.Contains("genkey failed", errorMessage);
            Assert.True(string.IsNullOrEmpty(publicKey));
            Assert.True(string.IsNullOrEmpty(privateKey));
        }

        [Fact]
        public async Task ListInterfacesAsync_Success_ReturnsListOfInterfaces()
        {
            // Arrange
            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg" && arguments == "show interfaces")
                {
                    return Task.FromResult(("wg0\nwg1\n", string.Empty, 0));
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (interfaces, errorMessage) = await service.ListInterfacesAsync();

            // Assert
            Assert.True(string.IsNullOrEmpty(errorMessage));
            Assert.NotNull(interfaces);
            Assert.Equal(2, interfaces.Count);
            Assert.Contains("wg0", interfaces);
            Assert.Contains("wg1", interfaces);
        }

        [Fact]
        public async Task ListInterfacesAsync_NoInterfaces_ReturnsEmptyList()
        {
            // Arrange
            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg" && arguments == "show interfaces")
                {
                    // This is how 'wg show interfaces' behaves when no interfaces are up.
                    return Task.FromResult((string.Empty, string.Empty, 0));
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (interfaces, errorMessage) = await service.ListInterfacesAsync();

            // Assert
            Assert.True(string.IsNullOrEmpty(errorMessage));
            Assert.NotNull(interfaces);
            Assert.Empty(interfaces);
        }
        
        [Fact]
        public async Task GetInterfaceDataAsync_Success_ReturnsRawData()
        {
            // Arrange
            var expectedData = "interface: wg0\npublic key: testkey\nlistening port: 12345";
            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg" && arguments == "show \"wg0\"")
                {
                    return Task.FromResult((expectedData, string.Empty, 0));
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (details, errorMessage) = await service.GetInterfaceDataAsync("wg0");

            // Assert
            Assert.True(string.IsNullOrEmpty(errorMessage));
            Assert.Equal(expectedData, details);
        }

        [Fact]
        public async Task GetInterfaceDataAsync_InterfaceNameValidation_ReturnsError()
        {
            // Arrange
            var service = new WireGuardService(); // No command execution needed

            // Act
            var (details, errorMessage) = await service.GetInterfaceDataAsync("invalid name with spaces");

            // Assert
            Assert.False(string.IsNullOrEmpty(errorMessage));
            Assert.Contains("Invalid characters in interface name", errorMessage);
            Assert.True(string.IsNullOrEmpty(details));
        }


        [Fact]
        public async Task StartInterfaceAsync_Success()
        {
            // Arrange
            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg-quick" && arguments == "up \"wg0\"")
                {
                    return Task.FromResult((string.Empty, string.Empty, 0));
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (success, errorMessage) = await service.StartInterfaceAsync("wg0");

            // Assert
            Assert.True(success);
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }
        
        [Fact]
        public async Task StopInterfaceAsync_Success()
        {
            // Arrange
            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg-quick" && arguments == "down \"wg0\"")
                {
                    return Task.FromResult((string.Empty, string.Empty, 0));
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (success, errorMessage) = await service.StopInterfaceAsync("wg0");

            // Assert
            Assert.True(success);
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public async Task AddPeerAsync_Success()
        {
            // Arrange
            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg" && arguments == "set \"wg0\" peer \"TestPublicKey=\" allowed-ips \"10.0.0.5/32\"")
                {
                    return Task.FromResult((string.Empty, string.Empty, 0));
                }
                return Task.FromResult((string.Empty, $"Unexpected command: {command} {arguments}", 1));
            });

            // Act
            var (success, errorMessage) = await service.AddPeerAsync("wg0", "TestPublicKey=", "10.0.0.5/32");

            // Assert
            Assert.True(success, $"Expected success, but got error: {errorMessage}");
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public async Task RemovePeerAsync_Success()
        {
            // Arrange
            var service = new TestableWireGuardService((command, arguments) =>
            {
                if (command == "wg" && arguments == "set \"wg0\" peer \"TestPublicKey=\" remove")
                {
                    return Task.FromResult((string.Empty, string.Empty, 0));
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (success, errorMessage) = await service.RemovePeerAsync("wg0", "TestPublicKey=");

            // Assert
            Assert.True(success);
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public async Task GetServerStatusAsync_RunningInterface_ReturnsStatus()
        {
            // Arrange
            var interfaceName = "wg0";
            var publicKey = "TestServerKey=";
            var listeningPort = "51820";
            var mockOutput = $"interface: {interfaceName}\n" +
                             $"  public key: {publicKey}\n" +
                             $"  listening port: {listeningPort}\n" +
                             $"  fwmark: 0xca6c\n\n" +
                             $"peer: PeerKey1=\n" +
                             $"  endpoint: 1.2.3.4:5678\n" +
                             $"  allowed ips: 10.0.0.1/32\n" +
                             $"  latest handshake: 1 minute, 2 seconds ago\n" +
                             $"  transfer: 1.23 GiB received, 4.56 GiB sent\n";

            var service = new TestableWireGuardService((cmd, args) =>
            {
                if (cmd == "wg" && args == $"show \"{interfaceName}\"")
                {
                    return Task.FromResult((mockOutput, string.Empty, 0));
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (status, pubKey, port, errorMsg) = await service.GetServerStatusAsync(interfaceName);

            // Assert
            Assert.True(string.IsNullOrEmpty(errorMsg), $"Error: {errorMsg}");
            Assert.Equal($"Interface '{interfaceName}' is running.", status);
            Assert.Equal(publicKey, pubKey);
            Assert.Equal(listeningPort, port);
        }

        [Fact]
        public async Task GetServerStatusAsync_InterfaceNotFound_ReturnsNotFoundStatus()
        {
            // Arrange
            var interfaceName = "wg1";
            var service = new TestableWireGuardService((cmd, args) =>
            {
                if (cmd == "wg" && args == $"show \"{interfaceName}\"")
                {
                    // Simulate "wg show <non_existent_interface>" behavior
                    return Task.FromResult((string.Empty, $"Interface {interfaceName} does not exist", 1)); 
                }
                return Task.FromResult((string.Empty, "Unexpected command", 1));
            });

            // Act
            var (status, pubKey, port, errorMsg) = await service.GetServerStatusAsync(interfaceName);
            
            // Assert
            // The service method should interpret this specific error as "not found" rather than a generic "Error"
            Assert.True(string.IsNullOrEmpty(errorMsg), "Error message should be empty for 'not found' status interpretation.");
            Assert.Equal($"Interface '{interfaceName}' not found or not running.", status);
            Assert.True(string.IsNullOrEmpty(pubKey));
            Assert.True(string.IsNullOrEmpty(port));
        }
    }
}
