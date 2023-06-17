using System.IO.Abstractions;

using Colors.Net;
using Colors.Net.StringColorExtensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Core;

using Polly;

using PowerArgs;

namespace NetworkPerspective.Sync.Cli
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class NpCLI
    {
        [HelpHook, ArgShortcut("--help"), ArgDescription("Shows this help")]
        public bool Help { get; set; }


        [ArgActionMethod, ArgDescription("Add or update entities / nodes in an existing network")]
        public async Task Entities(EntitiesOpts args)
        {
            var client = Program.Setup<EntitiesClient, EntitiesOpts>(args);
            await client!.Main();
        }


        [ArgActionMethod, ArgDescription("Add or update groups / reports in an existing network")]
        public async Task Groups(GroupsOpts args)
        {
            var client = Program.Setup<GroupsClient, GroupsOpts>(args);
            await client!.Main();
        }

        [ArgActionMethod, ArgDescription("Import interactions")]
        public async Task Interactions(InteractionsOpts args)
        {
            var client = Program.Setup<InteractionsClient, InteractionsOpts>(args);
            await client!.Main();
        }

        [ArgActionMethod, ArgDescription("Signal action")]
        public async Task Signal(SignalOpts args)
        {
            var client = Program.Setup<SignalClient, SignalOpts>(args);
            await client!.Main();
        }
    }

    internal sealed class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                await Args.InvokeActionAsync<NpCLI>(args);
                return 0;
            }
            catch (Exception e)
            {
                ColoredConsole.Error.WriteLine("\nError occurred!".Red());
                ColoredConsole.Error.WriteLine(e.Message.Red());
                ColoredConsole.Error.WriteLine(e.StackTrace);
                ColoredConsole.Error.WriteLine();
            }
            return 1;
        }

        public static TService Setup<TService, TOptions>(TOptions args)
            where TService : class
            where TOptions : class, ICommonOpts
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddTransient<TService>();
                    services.AddTransient(x => args);

                    services.AddTransient<IFileSystem, FileSystem>();

                    var httpClientBuilder = services
                        .AddHttpClient<ISyncHashedClient, SyncHashedClient>();

                    if (args.BaseUrl != null)
                        httpClientBuilder.ConfigureHttpClient(client => client.BaseAddress = new Uri(args.BaseUrl));

                    httpClientBuilder
                        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromMinutes(1),
                            TimeSpan.FromMinutes(5),
                            TimeSpan.FromMinutes(10),
                            TimeSpan.FromMinutes(10),
                            TimeSpan.FromMinutes(10),
                            TimeSpan.FromMinutes(30),
                            TimeSpan.FromMinutes(30),
                            TimeSpan.FromMinutes(30),
                            TimeSpan.FromMinutes(30),
                        }));
                });
            hostBuilder.ConfigureLogging((log) =>
            {
                log
                    .AddConsole()
                    .AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
            });
            var host = hostBuilder.Build();
            return host.Services.GetRequiredService<TService>();
        }
    }
}