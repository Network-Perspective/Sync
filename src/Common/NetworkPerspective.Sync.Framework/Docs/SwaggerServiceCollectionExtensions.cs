using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace NetworkPerspective.Sync.Framework.Docs
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurity();
                options.AddMetadata();
                options.AddXmlComments();
                options.EnableAnnotations();
                options.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["action"]}");
                options.IgnoreObsoleteActions();
            });

            services.AddSwaggerGenNewtonsoftSupport();
            //services.AddFluentValidationRulesToSwagger();

            return services;
        }

        private static void AddSecurity(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        }

        private static void AddMetadata(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "REST API Connector",
                Description = "Network Perspective REST API Connector",
                Contact = new OpenApiContact
                {
                    Name = "Network Perspective Team",
                    Email = string.Empty,
                    Url = new Uri("https://www.networkperspective.io/contact"),
                }
            });
        }

        private static void AddXmlComments(this SwaggerGenOptions options)
        {
            var xmlFileApplication = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
            var xmlPathApplication = Path.Combine(AppContext.BaseDirectory, xmlFileApplication);
            options.IncludeXmlComments(xmlPathApplication);

            var xmlFileFramework = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPathFramework = Path.Combine(AppContext.BaseDirectory, xmlFileFramework);
            options.IncludeXmlComments(xmlPathFramework);
        }
    }
}