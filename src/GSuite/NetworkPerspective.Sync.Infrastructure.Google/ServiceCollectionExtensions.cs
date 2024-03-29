﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Criterias;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleDataSource(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<GoogleConfig>(configurationSection);
            services.AddScoped<IAuthTester, AuthTester>();

            services.AddTransient<IRetryPolicyProvider, RetryPolicyProvider>();

            services.AddScoped<ICredentialsProvider, CredentialsProvider>();
            services.AddScoped<ICriteria, NonServiceUserCriteria>();

            services.AddScoped<IMailboxClient, MailboxClient>();
            services.AddScoped<ICalendarClient, CalendarClient>();
            services.AddScoped<IUsersClient, UsersClient>();
            services.AddScoped<IUserCalendarTimeZoneReader, UserCalendarTimeZoneReader>();

            services.AddScoped<IDataSource, GoogleFacade>();

            services.AddTransient<ISecretRotator, GoogleSecretsRotator>();
            return services;
        }
    }
}