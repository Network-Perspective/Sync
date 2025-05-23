﻿using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;
using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Fixtures;

public class GoogleClientFixture
{
    public const string ApplicationName = "gmail_app";
    public const string AdminEmail = "nptestuser12@worksmartona.com";

    public IImpesonificationCredentialsProvider CredentialProvider { get; }

    public GoogleClientFixture()
    {
        var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
        var secretRepository = new AzureKeyVaultClient(TokenCredentialFactory.Create(), secretRepositoryOptions, NullLogger<AzureKeyVaultClient>.Instance);
        CredentialProvider = new ImpersonificationCredentialsProvider(secretRepository);
    }
}