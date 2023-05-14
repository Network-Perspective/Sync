using System;

namespace NetworkPerspective.Sync.Slack.Dtos
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
        /// Enable/disable automatic channel join
        /// </summary>
        public bool AutoJoinChannels { get; set; } = true;

        /// <summary>
        /// Enable/disable channels names synchronization
        /// </summary>
        public bool SyncChannelsNames { get; set; } = false;
    }
}
