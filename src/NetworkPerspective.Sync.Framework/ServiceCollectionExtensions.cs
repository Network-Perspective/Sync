﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

using NetworkPerspective.Sync.Framework.Controllers;

using Newtonsoft.Json.Converters;

namespace NetworkPerspective.Sync.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFramework(this IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            mvcBuilder
                .AddApplicationPart(typeof(ErrorController).Assembly)
                .AddNewtonsoftJson(x => x.SerializerSettings.Converters.Add(new StringEnumConverter()));

            services.AddTransient<IErrorService, ErrorService>();

            return services;
        }

        public static IServiceCollection RemoveHttpClientLogging(this IServiceCollection services)
            => services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
    }
}