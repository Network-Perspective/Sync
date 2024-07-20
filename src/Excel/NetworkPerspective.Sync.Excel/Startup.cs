using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application;
using NetworkPerspective.Sync.Framework;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Framework.Docs;
using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.Infrastructure.Excel;
using NetworkPerspective.Sync.Infrastructure.Persistence;
using NetworkPerspective.Sync.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Excel
{
    public class Startup
    {
        private const string NetworkPerspectiveCoreConfigSection = "Infrastructure:NetworkPerspectiveCore";
        private const string SecretRepositoryClientBaseConfigSection = "Infrastructure";
        private const string ConnectorConfigSection = "Connector";

        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
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
                .AddDocumentation(typeof(Startup).Assembly)
                .AddApplication(_config.GetSection(ConnectorConfigSection))
                .AddExcel(_config.GetSection(ConnectorConfigSection))
                .AddSecretRepositoryClient(_config.GetSection(SecretRepositoryClientBaseConfigSection), healthChecksBuilder)
                .AddNetworkPerspectiveCore(_config.GetSection(NetworkPerspectiveCoreConfigSection), healthChecksBuilder)
                .AddPersistence(healthChecksBuilder)
                .AddFramework(mvcBuilder);


#if !DEBUG
            services.RemoveHttpClientLogging(); // need to be one of the last statements so handlers are not added by any other method
#else
            // services.AddNetworkPerspectiveCoreStub(_config.GetSection(NetworkPerspectiveCoreConfigSection));
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler(ErrorController.ErrorRoute);

            app.UseDocumentation();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

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