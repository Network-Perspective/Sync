using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Vaults.Ovh;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Configs;

using Newtonsoft.Json.Linq;

using VaultSharp;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Tests;

public class OvhVaultTests
{
    private const string BaseUrl = "https://vault.example.invalid/api/11111111-1111-1111-1111-111111111111";

    private static OvhVaultConfig Config() => new()
    {
        BaseUrl = BaseUrl,
        Token = "synthetic-test-token",
        MountPoint = "secret",
        TestSecretName = "synthetic-health"
    };

    private static IConfigurationRoot WorkerConfiguration(string sectionName = "OvhVault") => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            [$"{sectionName}:BaseUrl"] = BaseUrl,
            [$"{sectionName}:Token"] = "synthetic-test-token",
            [$"{sectionName}:MountPoint"] = "secret",
            [$"{sectionName}:TestSecretName"] = "synthetic-health"
        }).Build();

    private static OvhVaultClient Client(RecordingHandler handler, string baseUrl = BaseUrl)
    {
        var config = Config();
        config.BaseUrl = baseUrl;
        var settings = OvhVaultClient.CreateClientSettings(config);
        settings.MyHttpClientProviderFunc = _ => new HttpClient(handler);
        return new OvhVaultClient(Options.Create(config), NullLogger<OvhVaultClient>.Instance, new VaultClient(settings));
    }

    [Fact]
    public void RequiresTokenWithoutKubernetesFallbackAndDisablesRedirects()
    {
        var config = Config();
        new OvhVaultConfig.Validator("test").Validate(config).IsValid.Should().BeTrue();
        var settings = OvhVaultClient.CreateClientSettings(config);
        settings.UseVaultTokenHeaderInsteadOfAuthorizationHeader.Should().BeFalse();
        using var handler = new HttpClientHandler();
        settings.PostProcessHttpClientHandlerAction(handler);
        handler.AllowAutoRedirect.Should().BeFalse();
        using var sockets = new SocketsHttpHandler();
        settings.PostProcessHttpClientHandlerAction(sockets);
        sockets.AllowAutoRedirect.Should().BeFalse();

        config.Token = null;
        new OvhVaultConfig.Validator("test").Validate(config).IsValid.Should().BeFalse();
        Action create = () => OvhVaultClient.CreateClientSettings(config);
        create.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("http://vault.example.invalid/api/11111111-1111-1111-1111-111111111111")]
    [InlineData("https://vault.example.invalid")]
    [InlineData("https://vault.example.invalid/api/not-a-uuid")]
    [InlineData("https://user:password@vault.example.invalid/api/11111111-1111-1111-1111-111111111111")]
    [InlineData(BaseUrl + "?token=not-allowed")]
    [InlineData(BaseUrl + "#fragment")]
    public void RejectsUnsafeOrIncompleteEndpoint(string url)
    {
        var config = Config();
        config.BaseUrl = url;
        new OvhVaultConfig.Validator("test").Validate(config).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("OvhVault")]
    [InlineData("ovhvault")]
    public void BindsWorkerOvhSubsectionAndRegistersRealHealthCheck(string sectionName)
    {
        var config = WorkerConfiguration(sectionName);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVault(config, services.AddHealthChecks());
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IVault>().Should().BeOfType<OvhVaultClient>();
        provider.GetRequiredService<IOptions<OvhVaultConfig>>().Value.Should().BeEquivalentTo(Config());
        provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations
            .Should().ContainSingle(x => x.Name == "OVH Key-Vault");
        new VaultsConfig.Validator("test").Validate(config.Get<VaultsConfig>()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Token", "")]
    [InlineData("MountPoint", "other-mount")]
    [InlineData("BaseUrl", "http://vault.example.invalid")]
    [InlineData("TestSecretName", "")]
    public void InvalidOvhConfigurationFailsWithoutFallingBack(string field, string value)
    {
        var config = WorkerConfiguration();
        config[$"OvhVault:{field}"] = value;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVault(config, services.AddHealthChecks());
        using var provider = services.BuildServiceProvider();
        Action resolve = () => provider.GetRequiredService<IVault>();
        resolve.Should().Throw<OptionsValidationException>();
        new VaultsConfig.Validator("test").Validate(config.Get<VaultsConfig>()).IsValid.Should().BeFalse();
    }

    [Fact]
    public void OptionalCustomerAzureDecoratorStillApplies()
    {
        var config = WorkerConfiguration();
        config["ExternalAzureKeyVault:BaseUrl"] = "https://customer.example.invalid/";
        config["ExternalAzureKeyVault:TestSecretName"] = "synthetic-health";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVault(config, services.AddHealthChecks());
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IVault>().GetType().Name.Should().Be("ExternalAzureKeyVaultClient");
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, HealthStatus.Healthy)]
    [InlineData(HttpStatusCode.Forbidden, HealthStatus.Unhealthy)]
    [InlineData(HttpStatusCode.NotFound, HealthStatus.Unhealthy)]
    public async Task RegisteredHealthCheckActuallyReadsSecret(HttpStatusCode status, HealthStatus expected)
    {
        using var handler = new RecordingHandler(_ => Response(status, status == HttpStatusCode.OK
            ? "{\"data\":{\"data\":{\"secret\":\"synthetic-value\"}}}"
            : "{\"errors\":[\"synthetic-sensitive-echo\"]}"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVault(WorkerConfiguration(), services.AddHealthChecks());
        services.AddSingleton(Client(handler));
        using var provider = services.BuildServiceProvider();
        var report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();
        report.Status.Should().Be(expected);
        report.Entries["OVH Key-Vault"].Description.Should().NotContain("synthetic-sensitive-echo");
        handler.Requests.Should().ContainSingle(request => request.Method == HttpMethod.Get
            && request.Uri == BaseUrl + "/v1/secret/data/synthetic-health");
    }

    [Theory]
    [InlineData(BaseUrl)]
    [InlineData(BaseUrl + "/")]
    public async Task ReadsLatestSecretWithBearerTokenAndDomainPath(string baseUrl)
    {
        var currentValue = "synthetic-value";
        using var handler = new RecordingHandler(_ => Response(HttpStatusCode.OK,
            $"{{\"data\":{{\"data\":{{\"secret\":\"{currentValue}\"}}}}}}"));
        var client = Client(handler, baseUrl);
        (await client.GetSecretAsync("tokens/synthetic")).ToSystemString().Should().Be(currentValue);
        currentValue = "updated-value";
        (await client.GetSecretAsync("tokens/synthetic")).ToSystemString().Should().Be(currentValue);
        handler.Requests.Should().HaveCount(2);
        handler.Requests.Should().OnlyContain(request => request.Method == HttpMethod.Get
            && request.Uri == BaseUrl + "/v1/secret/data/tokens/synthetic"
            && request.Authorization == "Bearer synthetic-test-token");
    }

    [Fact]
    public async Task ExistingCachedVaultCanCreateReplaceAndDeleteWithoutVersionPreconditions()
    {
        using var handler = new RecordingHandler(request => request.Method == HttpMethod.Delete
            ? Response(HttpStatusCode.NoContent, "")
            : Response(HttpStatusCode.OK, "{\"data\":{\"version\":5}}"));
        var vault = new CachedVault(Client(handler), NullLogger<CachedVault>.Instance);
        var values = new[] { "value-ą-\"", "replacement" };
        foreach (var value in values)
            await vault.SetSecretAsync("synthetic", value.ToSecureString());
        await vault.RemoveSecretAsync("synthetic");

        handler.Requests.Should().HaveCount(3);
        for (var i = 0; i < values.Length; i++)
        {
            var request = handler.Requests[i];
            request.Method.Should().Be(HttpMethod.Post);
            request.Uri.Should().Be(BaseUrl + "/v1/secret/data/synthetic");
            var payload = JObject.Parse(request.Body);
            payload["data"]["secret"].Value<string>().Should().Be(values[i]);
            var cas = payload.SelectToken("options.cas");
            (cas == null || cas.Type == JTokenType.Null).Should().BeTrue();
        }
        handler.Requests.Last().Method.Should().Be(HttpMethod.Delete);
        handler.Requests.Last().Uri.Should().Be(BaseUrl + "/v1/secret/data/synthetic");
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task RejectedWriteIsNotRetriedAndResponseBodyIsNotExposed(HttpStatusCode status)
    {
        using var handler = new RecordingHandler(_ => Response(status, "{\"errors\":[\"synthetic-sensitive-echo\"]}"));
        Func<Task> act = () => Client(handler).SetSecretAsync("synthetic", "value".ToSecureString());
        var error = (await act.Should().ThrowAsync<VaultException>()).Which;
        error.ToString().Should().NotContain("synthetic-sensitive-echo");
        error.InnerException.Should().BeNull();
        handler.Requests.Should().ContainSingle();
    }

    [Theory]
    [InlineData('a', 64000)]
    [InlineData('ą', 32000)]
    [InlineData('"', 32000)]
    public async Task OversizedUtf8OrEscapedPayloadFailsBeforeAnyHttp(char value, int count)
    {
        using var handler = new RecordingHandler(_ => throw new Exception("Unexpected HTTP"));
        Func<Task> act = () => Client(handler).SetSecretAsync("synthetic", new string(value, count).ToSecureString());
        await act.Should().ThrowAsync<VaultException>().WithMessage("*64,000*");
        handler.Requests.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("../config")]
    [InlineData("/absolute")]
    [InlineData("a?version=1")]
    [InlineData("a//b")]
    public async Task RejectsUnsafeKeysWithoutHttp(string key)
    {
        using var handler = new RecordingHandler(_ => throw new Exception("Unexpected HTTP"));
        var client = Client(handler);
        Func<Task> read = () => client.GetSecretAsync(key);
        Func<Task> write = () => client.SetSecretAsync(key, "synthetic-value".ToSecureString());
        Func<Task> delete = () => client.RemoveSecretAsync(key);
        await read.Should().ThrowAsync<ArgumentException>();
        await write.Should().ThrowAsync<ArgumentException>();
        await delete.Should().ThrowAsync<ArgumentException>();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task PreCancelledOperationsNeverSendHttp()
    {
        using var handler = new RecordingHandler(_ => throw new Exception("Unexpected HTTP"));
        var client = Client(handler);
        var token = new CancellationToken(true);
        Func<Task> read = () => client.GetSecretAsync("synthetic", token);
        Func<Task> write = () => client.SetSecretAsync("synthetic", "value".ToSecureString(), token);
        Func<Task> delete = () => client.RemoveSecretAsync("synthetic", token);
        await read.Should().ThrowAsync<OperationCanceledException>();
        await write.Should().ThrowAsync<OperationCanceledException>();
        await delete.Should().ThrowAsync<OperationCanceledException>();
        handler.Requests.Should().BeEmpty();
    }

    private static HttpResponseMessage Response(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body)
    };

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(HttpMethod Method, string Uri, string Authorization, string Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add((request.Method, request.RequestUri.ToString(), request.Headers.Authorization?.ToString(),
                request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken)));
            return respond(request);
        }
    }
}