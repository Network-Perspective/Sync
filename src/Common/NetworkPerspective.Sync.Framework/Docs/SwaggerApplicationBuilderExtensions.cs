
using Microsoft.AspNetCore.Builder;

namespace NetworkPerspective.Sync.Framework.Docs
{
    public static class SwaggerApplicationBuilderExtensions
    {
        public static void UseDocumentation(this IApplicationBuilder app)
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Network Perspective Connector REST API V1");
            });
        }
    }
}