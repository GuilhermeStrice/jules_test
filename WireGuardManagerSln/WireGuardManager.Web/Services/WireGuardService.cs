using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WireGuardManager.Web.Services
{
    public class WireGuardService
    {
        protected virtual async Task<(string Output, string Error, int ExitCode)> ExecuteCommandAsync(string command, string arguments)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = false, // Default, but explicit
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync(); // Use WaitForExitAsync for async context

                    return (output.Trim(), error.Trim(), process.ExitCode);
                }
            }
            catch (Exception ex)
            {
                // Log the exception if a logger is available
                return (string.Empty, $"Exception during command execution: {ex.Message}", -1);
            }
        }

        public async Task<(string PublicKey, string PrivateKey, string ErrorMessage)> GenerateKeypairAsync()
        {
            try
            {
                // 1. Generate private key
                var genkeyResult = await ExecuteCommandAsync("wg", "genkey");
                if (genkeyResult.ExitCode != 0 || !string.IsNullOrEmpty(genkeyResult.Error))
                {
                    return (string.Empty, string.Empty, $"Error generating private key: {genkeyResult.Error} (Exit Code: {genkeyResult.ExitCode})");
                }
                string privateKey = genkeyResult.Output.Trim();
                if (string.IsNullOrEmpty(privateKey))
                {
                    return (string.Empty, string.Empty, "Failed to generate private key (empty output).");
                }

                // 2. Generate public key from private key
                // We need to pass the private key to the stdin of 'wg pubkey'
                ProcessStartInfo pubkeyPsi = new ProcessStartInfo
                {
                    FileName = "wg",
                    Arguments = "pubkey",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true, // Enable reading from stdin
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using (Process pubkeyProcess = new Process { StartInfo = pubkeyPsi })
                {
                    pubkeyProcess.Start();
                    // Write the private key to the process's standard input
                    using (StreamWriter sw = pubkeyProcess.StandardInput)
                    {
                        await sw.WriteAsync(privateKey);
                    }
                    
                    string publicKey = await pubkeyProcess.StandardOutput.ReadToEndAsync();
                    string pubkeyError = await pubkeyProcess.StandardError.ReadToEndAsync();
                    await pubkeyProcess.WaitForExitAsync();

                    if (pubkeyProcess.ExitCode != 0 || !string.IsNullOrEmpty(pubkeyError))
                    {
                        return (string.Empty, privateKey, $"Error generating public key: {pubkeyError} (Exit Code: {pubkeyProcess.ExitCode})");
                    }
                    
                    publicKey = publicKey.Trim();
                    if (string.IsNullOrEmpty(publicKey))
                    {
                         return (string.Empty, privateKey, "Failed to generate public key (empty output).");
                    }

                    return (publicKey, privateKey, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (string.Empty, string.Empty, $"Exception in GenerateKeypairAsync: {ex.Message}");
            }
        }

        public async Task<(List<string> Interfaces, string ErrorMessage)> ListInterfacesAsync()
        {
            try
            {
                // "wg show interfaces" lists active interface names, one per line.
                var result = await ExecuteCommandAsync("wg", "show interfaces");
                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Error))
                {
                    // wg show interfaces might return exit code 0 and error "No WireGuard interfaces found" if none are active.
                    // Or it might return non-zero if 'wg' itself has issues or no interfaces exist.
                    // For now, consider any error or non-zero exit as problematic for listing.
                    // However, if output is empty and exit code is 0, it means no interfaces.
                    if (string.IsNullOrEmpty(result.Output) && string.IsNullOrEmpty(result.Error) && result.ExitCode == 0)
                    {
                        return (new List<string>(), string.Empty); // No interfaces found, not an error
                    }
                    return (new List<string>(), $"Error listing interfaces: {result.Error} {result.Output} (Exit Code: {result.ExitCode})");
                }

                if (string.IsNullOrEmpty(result.Output))
                {
                    return (new List<string>(), string.Empty); // No interfaces found
                }

                var interfaces = result.Output.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim())
                                            .ToList();
                return (interfaces, string.Empty);
            }
            catch (Exception ex)
            {
                return (new List<string>(), $"Exception in ListInterfacesAsync: {ex.Message}");
            }
        }

        public async Task<(string InterfaceDetails, string ErrorMessage)> GetInterfaceDataAsync(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return (string.Empty, "Interface name cannot be empty.");
            }

            // Basic input validation to prevent command injection, though ProcessStartInfo helps.
            // Interface names are typically simple, but disallow problematic characters.
            if (interfaceName.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
            {
                return (string.Empty, $"Invalid characters in interface name: {interfaceName}");
            }

            try
            {
                var result = await ExecuteCommandAsync("wg", $"show \"{interfaceName}\""); // Enclose interface name in quotes
                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Error))
                {
                     // "wg show <interface>" will error if the interface doesn't exist.
                    return (string.Empty, $"Error getting data for interface '{interfaceName}': {result.Error} {result.Output} (Exit Code: {result.ExitCode})");
                }
                return (result.Output, string.Empty);
            }
            catch (Exception ex)
            {
                return (string.Empty, $"Exception in GetInterfaceDataAsync for interface '{interfaceName}': {ex.Message}");
            }
        }

        public async Task<(string Status, string PublicKey, string ListeningPort, string ErrorMessage)> GetServerStatusAsync(string interfaceName = "wg0")
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return ("Error", string.Empty, string.Empty, "Interface name for status check cannot be empty.");
            }
            // Basic input validation
            if (interfaceName.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
            {
                 return ("Error", string.Empty, string.Empty, $"Invalid characters in interface name: {interfaceName}");
            }

            try
            {
                var result = await ExecuteCommandAsync("wg", $"show \"{interfaceName}\"");

                // If 'wg show <interfaceName>' fails because the interface doesn't exist,
                // it's not a fundamental error for this method. It just means the specific interface isn't up.
                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Error))
                {
                    // Distinguish between "interface not found" and other errors
                    if (result.Error.Contains("No such device") || result.Output.Contains("No such device") ||
                        (result.Error.Contains("does not exist") && result.Error.Contains(interfaceName)))
                    {
                        return ($"Interface '{interfaceName}' not found or not running.", string.Empty, string.Empty, string.Empty);
                    }
                    // Other errors from wg show are more problematic
                    return ("Error", string.Empty, string.Empty, $"Error getting status for interface '{interfaceName}': {result.Error} {result.Output} (Exit Code: {result.ExitCode})");
                }

                if (string.IsNullOrEmpty(result.Output))
                {
                     return ($"Interface '{interfaceName}' not found or not running (empty output).", string.Empty, string.Empty, string.Empty);
                }

                // Simplified parsing for public key and listening port
                string publicKey = string.Empty;
                string listeningPort = string.Empty;

                var lines = result.Output.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("public key:"))
                    {
                        publicKey = trimmedLine.Substring("public key:".Length).Trim();
                    }
                    else if (trimmedLine.StartsWith("listening port:"))
                    {
                        listeningPort = trimmedLine.Substring("listening port:".Length).Trim();
                    }
                }

                if (!string.IsNullOrEmpty(publicKey) && !string.IsNullOrEmpty(listeningPort))
                {
                    return ($"Interface '{interfaceName}' is running.", publicKey, listeningPort, string.Empty);
                }
                else if (!string.IsNullOrEmpty(publicKey)) // Port might not always be shown if not actively listening for incoming?
                {
                     return ($"Interface '{interfaceName}' is configured (public key found).", publicKey, "N/A", string.Empty);
                }
                else
                {
                    return ($"Interface '{interfaceName}' status unknown (data found but key/port not parsed).", string.Empty, string.Empty, string.Empty);
                }

            }
            catch (Exception ex)
            {
                return ("Error", string.Empty, string.Empty, $"Exception in GetServerStatusAsync for '{interfaceName}': {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> StartInterfaceAsync(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return (false, "Interface name cannot be empty.");
            }
            if (interfaceName.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
            {
                return (false, $"Invalid characters in interface name: {interfaceName}");
            }

            try
            {
                // Assuming wg-quick is in PATH
                var result = await ExecuteCommandAsync("wg-quick", $"up \"{interfaceName}\"");
                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Error))
                {
                    // wg-quick can output to stderr even on success (e.g. "[#] ip link set અંગ્રેજી0 up")
                    // We should primarily rely on ExitCode, but non-empty Error is a strong signal of issues.
                    // Let's consider any stderr output as a potential issue for now, unless we identify specific benign messages.
                    string errorMessage = $"Error starting interface '{interfaceName}'. Exit Code: {result.ExitCode}.";
                    if (!string.IsNullOrEmpty(result.Output)) errorMessage += $" Output: {result.Output}";
                    if (!string.IsNullOrEmpty(result.Error)) errorMessage += $" Error: {result.Error}";
                    return (false, errorMessage);
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Exception in StartInterfaceAsync for '{interfaceName}': {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> StopInterfaceAsync(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return (false, "Interface name cannot be empty.");
            }
            if (interfaceName.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
            {
                return (false, $"Invalid characters in interface name: {interfaceName}");
            }

            try
            {
                // Assuming wg-quick is in PATH
                var result = await ExecuteCommandAsync("wg-quick", $"down \"{interfaceName}\"");
                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Error))
                {
                    string errorMessage = $"Error stopping interface '{interfaceName}'. Exit Code: {result.ExitCode}.";
                    if (!string.IsNullOrEmpty(result.Output)) errorMessage += $" Output: {result.Output}";
                    if (!string.IsNullOrEmpty(result.Error)) errorMessage += $" Error: {result.Error}";
                    return (false, errorMessage);
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Exception in StopInterfaceAsync for '{interfaceName}': {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddPeerAsync(string interfaceName, string peerPublicKey, string allowedIps)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return (false, "Interface name cannot be empty.");
            }
            if (interfaceName.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
            {
                return (false, $"Invalid characters in interface name: {interfaceName}");
            }
            if (string.IsNullOrWhiteSpace(peerPublicKey))
            {
                return (false, "Peer public key cannot be empty.");
            }
            // Basic validation for public key format (44 chars, base64) - can be improved
            if (peerPublicKey.Length != 44 || !peerPublicKey.EndsWith("=") || peerPublicKey.Any(c => !char.IsLetterOrDigit(c) && c != '+' && c != '/' && c != '='))
            {
                //return (false, $"Invalid peer public key format: {peerPublicKey}");
                // For now, allow more flexibility as wg tool will validate it.
            }
            if (string.IsNullOrWhiteSpace(allowedIps))
            {
                return (false, "Allowed IPs cannot be empty.");
            }
            // Basic validation for allowed IPs (e.g., CIDR format) - can be improved
            // Example: "10.0.0.2/32" or "10.0.0.0/24, 192.168.1.0/24"
            // For now, allow more flexibility as wg tool will validate it.

            try
            {
                string arguments = $"set \"{interfaceName}\" peer \"{peerPublicKey}\" allowed-ips \"{allowedIps}\"";
                var result = await ExecuteCommandAsync("wg", arguments);

                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Error))
                {
                    string errorMessage = $"Error adding peer to interface '{interfaceName}'. Exit Code: {result.ExitCode}.";
                    if (!string.IsNullOrEmpty(result.Output)) errorMessage += $" Output: {result.Output}"; // wg set might output to stdout on error
                    if (!string.IsNullOrEmpty(result.Error)) errorMessage += $" Error: {result.Error}";
                    return (false, errorMessage);
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Exception in AddPeerAsync for '{interfaceName}': {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RemovePeerAsync(string interfaceName, string peerPublicKey)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return (false, "Interface name cannot be empty.");
            }
            if (interfaceName.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
            {
                return (false, $"Invalid characters in interface name: {interfaceName}");
            }
            if (string.IsNullOrWhiteSpace(peerPublicKey))
            {
                return (false, "Peer public key cannot be empty.");
            }
            // Basic validation for public key format - can be improved
            if (peerPublicKey.Length != 44 || !peerPublicKey.EndsWith("=") || peerPublicKey.Any(c => !char.IsLetterOrDigit(c) && c != '+' && c != '/' && c != '='))
            {
                // For now, allow more flexibility as wg tool will validate it.
            }

            try
            {
                string arguments = $"set \"{interfaceName}\" peer \"{peerPublicKey}\" remove";
                var result = await ExecuteCommandAsync("wg", arguments);

                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Error))
                {
                    string errorMessage = $"Error removing peer from interface '{interfaceName}'. Exit Code: {result.ExitCode}.";
                     if (!string.IsNullOrEmpty(result.Output)) errorMessage += $" Output: {result.Output}";
                    if (!string.IsNullOrEmpty(result.Error)) errorMessage += $" Error: {result.Error}";
                    return (false, errorMessage);
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Exception in RemovePeerAsync for '{interfaceName}': {ex.Message}");
            }
        }

        public async Task<(string PeerConfig, string ErrorMessage)> GeneratePeerConfigAsync(
            string peerPrivateKey, 
            string peerPresharedKey, 
            List<string> peerDnsServers, 
            string interfaceAddress, 
            string serverPublicKey, 
            string serverEndpoint, 
            string serverAllowedIPs)
        {
            // 1. Input Validation
            if (string.IsNullOrWhiteSpace(peerPrivateKey))
            {
                return (null, "Peer private key is required.");
            }
            if (string.IsNullOrWhiteSpace(interfaceAddress))
            {
                return (null, "Interface address for the peer is required.");
            }
            if (string.IsNullOrWhiteSpace(serverPublicKey))
            {
                return (null, "Server public key is required.");
            }
            if (string.IsNullOrWhiteSpace(serverEndpoint))
            {
                return (null, "Server endpoint is required.");
            }
            if (string.IsNullOrWhiteSpace(serverAllowedIPs))
            {
                return (null, "Server allowed IPs are required.");
            }

            try
            {
                // 2. Configuration String Construction
                var sb = new System.Text.StringBuilder();

                sb.AppendLine("[Interface]");
                sb.AppendLine($"PrivateKey = {peerPrivateKey}");
                sb.AppendLine($"Address = {interfaceAddress}");

                // Handle optional DNS servers
                if (peerDnsServers != null && peerDnsServers.Any())
                {
                    // Validate DNS server IPs (basic validation)
                    foreach(var dns in peerDnsServers)
                    {
                        if (string.IsNullOrWhiteSpace(dns) || (!dns.Contains('.') && !dns.Contains(':'))) // Very basic check for IP-like format
                        {
                             return (null, $"Invalid DNS server address provided: '{dns}'.");
                        }
                    }
                    sb.AppendLine($"DNS = {string.Join(",", peerDnsServers)}");
                }
                
                sb.AppendLine(); // Blank line between sections

                sb.AppendLine("[Peer]");
                sb.AppendLine($"PublicKey = {serverPublicKey}");

                // Handle optional PresharedKey
                if (!string.IsNullOrWhiteSpace(peerPresharedKey))
                {
                    sb.AppendLine($"PresharedKey = {peerPresharedKey}");
                }

                sb.AppendLine($"AllowedIPs = {serverAllowedIPs}");
                sb.AppendLine($"Endpoint = {serverEndpoint}");
                
                // Add PersistentKeepalive as a sensible default
                sb.AppendLine("PersistentKeepalive = 25");

                // Simulating async work if needed, though StringBuilder is sync
                await Task.Yield(); // Ensures the method is awaitable and behaves like other async methods

                return (sb.ToString(), string.Empty);
            }
            catch (Exception ex)
            {
                // Log exception if a logger is available
                return (null, $"An unexpected error occurred while generating peer configuration: {ex.Message}");
            }
        }
    }
}
