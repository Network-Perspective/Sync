using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Criterias;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogle(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<GoogleConfig>(configurationSection);
            //services.AddScoped<IAuthTester, AuthTester>();

            services.AddTransient<IRetryPolicyProvider, RetryPolicyProvider>();

            services.AddScoped<ICredentialsProvider, CredentialsProvider>();
            services.AddScoped<ICriteria, NonServiceUserCriteria>();

            services.AddScoped<IMailboxClient, MailboxClient>();
            services.AddScoped<ICalendarClient, CalendarClient>();
            services.AddScoped<IUsersClient, UsersClient>();
            services.AddScoped<IUserCalendarTimeZoneReader, UserCalendarTimeZoneReader>();

            services.AddScoped<IDataSource, GoogleFacade>();

            return services;
        }
    }
}