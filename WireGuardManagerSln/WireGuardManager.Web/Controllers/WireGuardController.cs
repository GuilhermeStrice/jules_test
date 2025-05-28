using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WireGuardManager.Web.Services;
using WireGuardManager.Web.Models; // For request models

namespace WireGuardManager.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WireGuardController : ControllerBase
    {
        private readonly WireGuardService _wireGuardService;

        public WireGuardController(WireGuardService wireGuardService)
        {
            _wireGuardService = wireGuardService ?? throw new ArgumentNullException(nameof(wireGuardService));
        }

        // GET /api/wireguard/status?interfaceName=wg0
        [HttpGet("status")]
        public async Task<IActionResult> GetServerStatus([FromQuery] string interfaceName = "wg0")
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return BadRequest("Interface name cannot be empty.");
            }

            var (status, publicKey, listeningPort, errorMessage) = await _wireGuardService.GetServerStatusAsync(interfaceName);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Distinguish between "interface not found" which might be OK, vs. actual errors
                if (status == "Error") // Indicates a more severe error from the service method
                {
                    return StatusCode(500, new { Message = errorMessage });
                }
                // If status indicates not found but no error message, it's still a form of "success" for the query
                // e.g. "Interface 'wgX' not found or not running."
            }
            
            // If status indicates not running, but there's no specific error message, it's an OK response with that status.
            if (status.Contains("not found or not running") && string.IsNullOrEmpty(errorMessage))
            {
                 return Ok(new { Status = status, PublicKey = publicKey, ListeningPort = listeningPort });
            }

            return Ok(new { Status = status, PublicKey = publicKey, ListeningPort = listeningPort });
        }

        // GET /api/wireguard/interfaces
        [HttpGet("interfaces")]
        public async Task<IActionResult> ListInterfaces()
        {
            var (interfaces, errorMessage) = await _wireGuardService.ListInterfacesAsync();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return StatusCode(500, new { Message = errorMessage });
            }

            return Ok(interfaces);
        }

        // GET /api/wireguard/interfaces/{interfaceName}
        [HttpGet("interfaces/{interfaceName}")]
        public async Task<IActionResult> GetInterfaceData(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return BadRequest("Interface name cannot be empty.");
            }

            var (interfaceDetails, errorMessage) = await _wireGuardService.GetInterfaceDataAsync(interfaceName);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Heuristic: "No such device" or "does not exist" implies NotFound
                if (errorMessage.Contains("No such device") || errorMessage.Contains("does not exist"))
                {
                    return NotFound(new { Message = errorMessage });
                }
                return StatusCode(500, new { Message = errorMessage });
            }
            
            if (string.IsNullOrEmpty(interfaceDetails) && string.IsNullOrEmpty(errorMessage))
            {
                // This case might mean the interface exists but has no data, or doesn't exist cleanly.
                // Depending on `wg show <non_existent_if>` behavior, this might be covered by error check.
                // If it can return no error and no details for a non-existent interface, then NotFound is appropriate.
                return NotFound(new {Message = $"Interface '{interfaceName}' not found or has no configuration."});
            }

            return Ok(new { Details = interfaceDetails });
        }

        // POST /api/wireguard/interfaces/{interfaceName}/start
        [HttpPost("interfaces/{interfaceName}/start")]
        public async Task<IActionResult> StartInterface(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return BadRequest("Interface name cannot be empty.");
            }

            var (success, errorMessage) = await _wireGuardService.StartInterfaceAsync(interfaceName);

            if (!success)
            {
                // Determine if it's a client error (e.g., interface config missing) or server error
                // This often requires more detailed error parsing from wg-quick.
                // For now, a general 500 or specific 400 if error message implies client fault.
                if (errorMessage.Contains("Cannot find device")) // Example
                {
                    return NotFound(new { Message = errorMessage });
                }
                return BadRequest(new { Message = errorMessage }); // Or StatusCode(500, ...) if it's a server-side execution issue
            }

            return Ok(new { Message = $"Interface '{interfaceName}' started successfully." });
        }

        // POST /api/wireguard/interfaces/{interfaceName}/stop
        [HttpPost("interfaces/{interfaceName}/stop")]
        public async Task<IActionResult> StopInterface(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return BadRequest("Interface name cannot be empty.");
            }

            var (success, errorMessage) = await _wireGuardService.StopInterfaceAsync(interfaceName);

            if (!success)
            {
                 if (errorMessage.Contains("Cannot find device")) // Example
                {
                    return NotFound(new { Message = errorMessage });
                }
                return BadRequest(new { Message = errorMessage });
            }

            return Ok(new { Message = $"Interface '{interfaceName}' stopped successfully." });
        }

        // GET /api/wireguard/generate-keys
        [HttpGet("generate-keys")]
        public async Task<IActionResult> GenerateKeys()
        {
            var (publicKey, privateKey, errorMessage) = await _wireGuardService.GenerateKeypairAsync();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return StatusCode(500, new { Message = errorMessage });
            }
            
            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
            {
                return StatusCode(500, new { Message = "Failed to generate key pair (empty keys returned)." });
            }

            return Ok(new { PublicKey = publicKey, PrivateKey = privateKey });
        }

        // POST /api/wireguard/interfaces/{interfaceName}/peers
        [HttpPost("interfaces/{interfaceName}/peers")]
        public async Task<IActionResult> AddPeer(string interfaceName, [FromBody] AddPeerRequest request)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return BadRequest("Interface name cannot be empty.");
            }
            if (!ModelState.IsValid) // Validates [Required] attributes in AddPeerRequest
            {
                return BadRequest(ModelState);
            }

            var (success, errorMessage) = await _wireGuardService.AddPeerAsync(interfaceName, request.PublicKey, request.AllowedIPs);

            if (!success)
            {
                // Check for specific errors that might warrant a 404 (e.g. interface not found)
                // or 400 (e.g. invalid public key format).
                if (errorMessage.Contains("Cannot find device") || errorMessage.Contains("No such device"))
                {
                     return NotFound(new { Message = $"Interface '{interfaceName}' not found. Error: {errorMessage}" });
                }
                if (errorMessage.Contains("Invalid public key") || errorMessage.Contains("Invalid IP address"))
                {
                    return BadRequest(new { Message = errorMessage });
                }
                return StatusCode(500, new { Message = errorMessage });
            }

            return Ok(new { Message = "Peer added successfully." });
        }

        // DELETE /api/wireguard/interfaces/{interfaceName}/peers/{peerPublicKey}
        [HttpDelete("interfaces/{interfaceName}/peers/{peerPublicKey}")]
        public async Task<IActionResult> RemovePeer(string interfaceName, string peerPublicKey)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return BadRequest("Interface name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(peerPublicKey))
            {
                return BadRequest("Peer public key cannot be empty.");
            }
            
            // ASP.NET Core automatically URL decodes path parameters.
            // However, WireGuard public keys can contain '+' which becomes a space if not properly encoded client-side
            // and then URL decoded. Ensure clients are encoding it (e.g. as %2B).
            // If issues persist, manual handling might be needed, but usually it's fine.

            var (success, errorMessage) = await _wireGuardService.RemovePeerAsync(interfaceName, peerPublicKey);

            if (!success)
            {
                if (errorMessage.Contains("Cannot find device") || errorMessage.Contains("No such device"))
                {
                     return NotFound(new { Message = $"Interface '{interfaceName}' not found. Error: {errorMessage}" });
                }
                if (errorMessage.Contains("Peer not found") || errorMessage.Contains("No such peer")) // Hypothetical error messages
                {
                    return NotFound(new { Message = $"Peer with public key '{peerPublicKey}' not found on interface '{interfaceName}'. Error: {errorMessage}"});
                }
                return StatusCode(500, new { Message = errorMessage });
            }

            return Ok(new { Message = "Peer removed successfully." });
        }

        // POST /api/wireguard/generate-peer-config
        [HttpPost("generate-peer-config")]
        public async Task<IActionResult> GeneratePeerConfig([FromBody] GeneratePeerConfigRequest request)
        {
            if (!ModelState.IsValid) // Validates [Required] attributes
            {
                return BadRequest(ModelState);
            }

            var (peerConfig, errorMessage) = await _wireGuardService.GeneratePeerConfigAsync(
                request.PeerPrivateKey,
                request.PeerPresharedKey,
                request.PeerDnsServers,
                request.InterfaceAddress,
                request.ServerPublicKey,
                request.ServerEndpoint,
                request.ServerAllowedIPs
            );

            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Errors from GeneratePeerConfigAsync are typically validation errors
                return BadRequest(new { Message = errorMessage });
            }
            
            if (string.IsNullOrEmpty(peerConfig))
            {
                 return StatusCode(500, new { Message = "Failed to generate peer configuration (empty config returned)." });
            }

            return Ok(new { PeerConfig = peerConfig });
        }
    }
}
