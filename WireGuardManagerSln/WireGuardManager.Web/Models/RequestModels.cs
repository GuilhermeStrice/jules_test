using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WireGuardManager.Web.Models
{
    public record AddPeerRequest(
        [Required] string PublicKey,
        [Required] string AllowedIPs
    );

    public record GeneratePeerConfigRequest(
        [Required] string PeerPrivateKey,
        string? PeerPresharedKey, // Optional
        List<string>? PeerDnsServers, // Optional
        [Required] string InterfaceAddress,
        [Required] string ServerPublicKey,
        [Required] string ServerEndpoint,
        [Required] string ServerAllowedIPs
    );
}
