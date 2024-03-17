using Microsoft.AspNetCore.Builder;

#if DEBUG
using System.Collections.Generic;

using Microsoft.OpenApi.Models;
#endif

namespace NetworkPerspective.Sync.Framework.Docs
{
    public static class SwaggerApplicationBuilderExtensions
    {
        public static void UseDocumentation(this IApplicationBuilder app)
        {
#if DEBUG
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var apiHostUrl = $"{httpReq.Scheme}://{httpReq.Host.Value}";
                    swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = apiHostUrl } };
                });
            });
#endif
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Network Perspective Connector REST API V1");
            });
        }
    }
}