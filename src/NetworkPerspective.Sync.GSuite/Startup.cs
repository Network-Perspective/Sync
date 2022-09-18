using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Scheduler;
using NetworkPerspective.Sync.Framework;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Framework.Docs;
using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.Infrastructure.Google;
using NetworkPerspective.Sync.Infrastructure.Persistence;
using NetworkPerspective.Sync.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.GSuite
{
    public class Startup
    {
        private const string GoogleConfigSection = "Infrastructure:Google";
        private const string NetworkPerspectiveCoreConfigSection = "Infrastructure:NetworkPerspectiveCore";
        private const string AzureKeyVaultConfigSection = "Infrastructure:AzureKeyVault";
        private const string SchedulerConfigSection = "Connector:Scheduler";
        private const string ConnectorConfigSection = "Connector";

        private readonly string _dbConnectionString;

        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
            _dbConnectionString = config.GetConnectionString("Database");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddStartupDbInitializer();

            var mvcBuilder = services
                .AddControllers(options =>
                {
                    options.OutputFormatters.RemoveType<StringOutputFormatter>();
                });

            var healthChecksBuilder = services
                .AddHealthChecks();

            services
                .AddDocumentation()
                .AddApplication(_config.GetSection(ConnectorConfigSection))
                .AddGoogleDataSource(_config.GetSection(GoogleConfigSection))
                .AddSecretStorage(_config.GetSection(AzureKeyVaultConfigSection), healthChecksBuilder)
                .AddNetworkPerspectiveCore(_config.GetSection(NetworkPerspectiveCoreConfigSection), healthChecksBuilder)
                .AddScheduler(_config.GetSection(SchedulerConfigSection), _dbConnectionString)
                .AddPersistence(healthChecksBuilder)
                .AddFramework(mvcBuilder);


#if !DEBUG
            services.RemoveHttpClientLogging();
#else
            services.AddSingleton<INetworkPerspectiveCore, NetworkPerspectiveCoreStub>();
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler(ErrorController.ErrorRoute);

            app.UseDocumentation();

            app.UseRouting();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
    }
}