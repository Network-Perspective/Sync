using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;
using NetworkPerspective.Sync.Orchestrator.Auth.Worker;
using NetworkPerspective.Sync.Orchestrator.Hubs.V1;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace NetworkPerspective.Sync.Orchestrator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHub(this IServiceCollection services)
    {
        services
            .AddSingleton<WorkerHubV1>()
            .AddSingleton<IWorkerRouter>(sp => sp.GetRequiredService<WorkerHubV1>())
            .AddSignalR();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication(ApiKeyAuthOptions.DefaultScheme)
            .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(ApiKeyAuthOptions.DefaultScheme, options => { })
            .AddScheme<WorkerAuthOptions, WorkerAuthHandler>(WorkerAuthOptions.DefaultScheme, options => { });

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
            Title = "REST API Orchestrator",
            Description = "Network Perspective REST API Orchestrator",
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
        var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
        options.IncludeXmlComments(xmlFilePath);
    }
}