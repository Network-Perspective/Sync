using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Services;

using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace NetworkPerspective.Sync.Scheduler
{
    internal class CustomJobFactory : MicrosoftDependencyInjectionJobFactory
    {
        public CustomJobFactory(IServiceProvider serviceProvider, IOptions<QuartzOptions> options) : base(serviceProvider, options)
        { }

        protected override void ConfigureScope(IServiceScope scope, TriggerFiredBundle bundle, IScheduler scheduler)
        {
            base.ConfigureScope(scope, bundle, scheduler);

            var connectorInfo = GetConnectorInfo(bundle.JobDetail);

            var initializer = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
            initializer.Initialize(connectorInfo);
        }

        private static ConnectorInfo GetConnectorInfo(IJobDetail jobDetail)
        {
            var value = jobDetail.Key.Name.Split(":");

            if (value.Length != 2)
                throw new Exception("Invalid Job Key Name. It should consists of two parts separated by ':' sign");

            if (!Guid.TryParse(value[0], out Guid connectorId))
                throw new Exception("Invalid Job Key Name. First part (separated by ':' sign) should be a Guid representing connector id");

            if (!Guid.TryParse(value[1], out Guid networkId))
                throw new Exception("Invalid Job Key Name. Second part (separated by ':' sign) should be a Guid representing network id");

            return new ConnectorInfo(connectorId, networkId);
        }
    }
}