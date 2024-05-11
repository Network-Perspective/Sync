﻿using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth;
using NetworkPerspective.Sync.Orchestrator.Hubs;

namespace NetworkPerspective.Sync.Orchestrator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHub(this IServiceCollection services)
    {
        services
            .AddSingleton<WorkerHubV1>()
            .AddSignalR();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication(ServiceAuthOptions.DefaultScheme)
            .AddScheme<ServiceAuthOptions, ServiceAuthHandler>(ServiceAuthOptions.DefaultScheme, options => { });

        services
            .AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

        services
            .AddTransient<IErrorService, ErrorService>();

        return services;
    }

    public static IServiceCollection AddDocumentation(this IServiceCollection services, Assembly serviceAssembly)
    {
        services.AddOpenApiDocument(configure =>
        {
            configure.Title = "Service ";
        });

        //services.AddSwaggerGen(options =>
        //{
        //    options.AddSecurity();
        //    options.AddMetadata();
        //    options.AddXmlComments(serviceAssembly);
        //    options.EnableAnnotations();
        //    options.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["action"]}");
        //    options.IgnoreObsoleteActions();
        //});

        //services.AddSwaggerGenNewtonsoftSupport();
        //services.AddFluentValidationRulesToSwagger();

        return services;
    }

    //private static void AddSecurity(this SwaggerGenOptions options)
    //{
    //    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    //    {
    //        Name = "Authorization",
    //        In = ParameterLocation.Header,
    //        Type = SecuritySchemeType.ApiKey,
    //        Scheme = "Bearer"
    //    });

    //    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    //    {
    //        {
    //            new OpenApiSecurityScheme
    //            {
    //                Reference = new OpenApiReference
    //                {
    //                    Type = ReferenceType.SecurityScheme,
    //                    Id = "Bearer"
    //                },
    //                Scheme = "oauth2",
    //                Name = "Bearer",
    //                In = ParameterLocation.Header,
    //            },
    //            new List<string>()
    //        }
    //    });
    //}

    //private static void AddMetadata(this SwaggerGenOptions options)
    //{
    //    options.SwaggerDoc("v1", new OpenApiInfo
    //    {
    //        Version = "v1",
    //        Title = "REST API Connector",
    //        Description = "Network Perspective REST API Connector",
    //        Contact = new OpenApiContact
    //        {
    //            Name = "Network Perspective Team",
    //            Email = string.Empty,
    //            Url = new Uri("https://www.networkperspective.io/contact"),
    //        }
    //    });
    //}

    //private static void AddXmlComments(this SwaggerGenOptions options, Assembly serviceAssembly)
    //{
    //    var xmlFileApplication = $"{serviceAssembly.GetName().Name}.xml";
    //    var xmlPathApplication = Path.Combine(AppContext.BaseDirectory, xmlFileApplication);
    //    options.IncludeXmlComments(xmlPathApplication);

    //    var xmlFileFramework = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //    var xmlPathFramework = Path.Combine(AppContext.BaseDirectory, xmlFileFramework);
    //    options.IncludeXmlComments(xmlPathFramework);
    //}
}