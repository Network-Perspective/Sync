using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google
{
    public class GoogleNetworkProperties : ConnectorProperties
    {
        private new const bool DefaultSyncGroups = true;

        public string AdminEmail { get; set; }
        public string Domain { get; set; }

        public GoogleNetworkProperties(string adminEmail, Uri externalKeyVaultUri) : base(DefaultSyncGroups, DefaultSyncChannelsNames, externalKeyVaultUri)
        {
            AdminEmail = adminEmail;
            Domain = AdminEmail.Split('@').Skip(1).Single();
        }

        public GoogleNetworkProperties() : base(DefaultSyncGroups, DefaultSyncChannelsNames, null)
        { }

        public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
        {
            base.Bind(properties);

            if (properties.Any(x => x.Key == nameof(AdminEmail)))
                AdminEmail = properties.Single(x => x.Key == nameof(AdminEmail)).Value;

            if (properties.Any(x => x.Key == nameof(Domain)))
                Domain = properties.Single(x => x.Key == nameof(Domain)).Value;

        }

        public override IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            var props = new List<KeyValuePair<string, string>>
            {
                new(nameof(AdminEmail), AdminEmail),
                new(nameof(Domain), Domain.ToString())
            };

            props.AddRange(base.GetAll());

            return props;
        }
    }
}