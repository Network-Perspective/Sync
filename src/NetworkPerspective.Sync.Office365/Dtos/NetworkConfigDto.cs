using System;
using System.ComponentModel.DataAnnotations;

namespace NetworkPerspective.Sync.Office365.Dtos
{
    /// <summary>
    /// Network configuration
    /// </summary>
    public class NetworkConfigDto
    {
        /// <summary>
        /// External Key Vault Uri (optional) in case it's not provided the internal key vault is used
        /// </summary>
        public Uri ExternalKeyVaultUri { get; set; } = null;

        /// <summary>
        /// Enable/disable MS Teams synchronization
        /// </summary>
        public bool SyncMsTeams { get; set; }
    }
}
