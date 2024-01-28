using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

            var networkId = GetNetworkId(bundle.JobDetail);

            if (networkId != Guid.Empty)
            {
                var initializer = scope.ServiceProvider.GetRequiredService<INetworkIdInitializer>();
                initializer.Initialize(networkId);
            }
        }

        private static Guid GetNetworkId(IJobDetail jobDetail)
            => Guid.TryParse(jobDetail.Key.Name, out Guid networkId) ? networkId : Guid.Empty;
    }
}