
using System.Collections.Generic;

using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace NetworkPerspective.Sync.Framework.Docs
{
    public static class SwaggerApplicationBuilderExtensions
    {
        public static void UseDocumentation(this IApplicationBuilder app)
        {
            // this breaks ci tests for some reason
            //
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