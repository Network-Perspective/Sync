using System;

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
        public bool SyncMsTeams { get; set; } = true;

        /// <summary>
        /// Enable/disable MS Teams Chats synchronization (applicable only if SyncMsTeams is enabled)
        /// </summary>
        public bool SyncChats { get; set; } = true;

        ///// <summary>
        ///// Enable/disable channels names synchronization (applicable only if SyncMsTeams is enabled)
        ///// </summary>
        //public bool SyncChannelsNames { get; set; } = false;

        ///// <summary>
        ///// Enable/disable user's group access synchronization
        ///// </summary>
        //public bool SyncGroupAccess { get; set; } = false;


    }
}