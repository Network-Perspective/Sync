//using System;
//using System.Security;
//using System.Text;
//using System.Threading.Tasks;

//using Google.Apis.Auth.OAuth2;
//using Google.Cloud.Iam.Admin.V1;

//using Grpc.Auth;

//using Microsoft.Extensions.Logging;

//using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
//using NetworkPerspective.Sync.Utils.Extensions;

//using Newtonsoft.Json.Linq;

//namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

//public class GoogleSecretsRotator(ILogger<GoogleSecretsRotator> logger, IVault secretRepository) : ISecretRotator
//{
//    private readonly ILogger<GoogleSecretsRotator> _logger = logger;
//    private readonly IVault _secretRepository = secretRepository;

//    public async Task RotateSecrets()
//    {
//        _logger.LogInformation("Rotating Google secrets");
//        try
//        {
//            var googleKey = await secretRepository.GetSecretAsync(GoogleKeys.TokenKey);
//            var credential = GoogleCredential
//                .FromJson(googleKey.ToSystemString())
//                .CreateScoped("https://www.googleapis.com/auth/cloud-platform");

//            // Create the IAM service client
//            var iamClient = await new IAMClientBuilder() { ChannelCredentials = credential.ToChannelCredentials() }
//                .BuildAsync();

//            (string serviceAccountEmail, string privateKeyId) = ReadEmailAndKeyId(googleKey);
//            var fullServiceAccountName = $"projects/-/serviceAccounts/{serviceAccountEmail}";

//            // Create a new key for the service account
//            ServiceAccountKey response = await iamClient.CreateServiceAccountKeyAsync(
//                fullServiceAccountName,
//                ServiceAccountPrivateKeyType.TypeGoogleCredentialsFile,
//                ServiceAccountKeyAlgorithm.KeyAlgUnspecified
//            );

//            // save the new key to the secret store
//            string newKeyContent = Encoding.UTF8.GetString(response.PrivateKeyData.ToByteArray());
//            await secretRepository.SetSecretAsync(GoogleKeys.TokenKey, newKeyContent.ToSecureString());

//            // delete the old key
//            var oldKeyName = $"projects/-/serviceAccounts/{serviceAccountEmail}/keys/{privateKeyId}";
//            await iamClient.DeleteServiceAccountKeyAsync(oldKeyName);

//            // Write the service account key to a file
//            _logger.LogInformation("Google key rotated");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to rotate Google key");
//            throw;
//        }
//    }

//    private (string clientEmail, string privateKeyId) ReadEmailAndKeyId(SecureString googleKey)
//    {
//        var jsonObject = JObject.Parse(googleKey.ToSystemString());

//        var clientEmail = jsonObject["client_email"]?.ToString();
//        if (clientEmail == null)
//            throw new ArgumentException("Google key missing client_email field");

//        var privateKeyId = jsonObject["private_key_id"]?.ToString();
//        if (privateKeyId == null)
//            throw new ArgumentException("Google key missing private_key_id field");

//        return (clientEmail, privateKeyId);
//    }
//}

// TODO what to do with rotators?