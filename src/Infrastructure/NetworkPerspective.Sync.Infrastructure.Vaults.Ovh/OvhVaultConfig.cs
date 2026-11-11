using System;

using FluentValidation;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.Ovh;

public class OvhVaultConfig
{
    public string BaseUrl { get; set; }
    public string TestSecretName { get; set; }
    public string Token { get; set; }
    public string MountPoint { get; set; }

    private static bool IsOvhEndpoint(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps
            || uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0)
            return false;

        var segments = uri.AbsolutePath.Trim('/').Split('/');
        return segments.Length == 2 && segments[0] == "api" && Guid.TryParse(segments[1], out _);
    }

    public class Validator : AbstractValidator<OvhVaultConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.BaseUrl)
                .Must(IsOvhEndpoint)
                .WithName($"{configPath}__{nameof(BaseUrl)}")
                .WithMessage("'{PropertyName}' must be an HTTPS origin followed by /api/<domain UUID>, without credentials, query or fragment.");

            RuleFor(x => x.TestSecretName)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(TestSecretName)}")
                .WithMessage("Please provide test secret name with environment variable '{PropertyName}'");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(Token)}")
                .WithMessage("Please provide OVH runtime token with environment variable '{PropertyName}'");

            RuleFor(x => x.MountPoint)
                .Equal("secret")
                .WithName($"{configPath}__{nameof(MountPoint)}")
                .WithMessage("'{PropertyName}' must be 'secret' for OVH KV2.");
        }
    }
}