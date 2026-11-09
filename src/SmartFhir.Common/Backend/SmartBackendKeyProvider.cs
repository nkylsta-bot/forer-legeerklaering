using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace SmartFhir.Common.Backend
{
    /// <summary>
    /// The RSA key the app signs SMART Backend Services client assertions with (private_key_jwt).
    /// The public half is published by SmartLaunchControllerBase at /{org}/{app}/smart/jwks.json,
    /// so the EHR registers the app with a jwksUri instead of a copied key.
    ///
    /// The key is read from SmartOnFhir:Backend:PrivateKeyJwk (a private JWK as JSON, meant for a
    /// Key Vault secret) or from the file at SmartOnFhir:Backend:PrivateKeyPath. In Development,
    /// with neither set, a key is created once and kept in the user's local app data folder, so it
    /// survives restarts. Outside Development a missing key is an error when the key is first used.
    /// </summary>
    public sealed class SmartBackendKeyProvider : IDisposable
    {
        private readonly Lazy<(RSA Rsa, string KeyId)> _key;

        public SmartBackendKeyProvider(
            IConfiguration config,
            IHostEnvironment env,
            ILogger<SmartBackendKeyProvider> logger
        )
        {
            _key = new Lazy<(RSA, string)>(() => LoadOrCreate(config, env, logger));
        }

        public string KeyId => _key.Value.KeyId;

        /// <summary>Credentials for signing client assertions with RS384, the algorithm nav-epj requires.</summary>
        public SigningCredentials SigningCredentials =>
            new(new RsaSecurityKey(_key.Value.Rsa) { KeyId = KeyId }, SecurityAlgorithms.RsaSha384);

        /// <summary>The public JWK Set, with the private parts left out.</summary>
        public object PublicJwks
        {
            get
            {
                var p = _key.Value.Rsa.ExportParameters(includePrivateParameters: false);
                return new
                {
                    keys = new[]
                    {
                        new
                        {
                            kty = "RSA",
                            use = "sig",
                            alg = "RS384",
                            kid = KeyId,
                            n = Base64UrlEncoder.Encode(p.Modulus),
                            e = Base64UrlEncoder.Encode(p.Exponent),
                        },
                    },
                };
            }
        }

        public void Dispose()
        {
            if (_key.IsValueCreated)
                _key.Value.Rsa.Dispose();
        }

        private static (RSA, string) LoadOrCreate(IConfiguration config, IHostEnvironment env, ILogger logger)
        {
            var inlineJwk = config["SmartOnFhir:Backend:PrivateKeyJwk"];
            if (!string.IsNullOrWhiteSpace(inlineJwk))
                return FromJwk(inlineJwk);

            var path = config["SmartOnFhir:Backend:PrivateKeyPath"];
            if (string.IsNullOrWhiteSpace(path))
            {
                if (!env.IsDevelopment())
                {
                    throw new InvalidOperationException(
                        "SmartOnFhir:Backend:PrivateKeyJwk or SmartOnFhir:Backend:PrivateKeyPath must be set outside Development"
                    );
                }

                var clientId = config["SmartOnFhir:Backend:ClientId"] ?? "smart-backend";
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "smartfhir-common",
                    $"{clientId}-key.json"
                );
            }

            if (File.Exists(path))
                return FromJwk(File.ReadAllText(path));

            if (!env.IsDevelopment())
                throw new InvalidOperationException($"SMART backend key not found at {path}");

            var rsa = RSA.Create(2048);
            var kid = "dev-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToLowerInvariant();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(PrivateJwk.From(rsa, kid)));
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            logger.LogInformation("Created a development SMART backend key {KeyId} at {Path}", kid, path);
            return (rsa, kid);
        }

        private static (RSA, string) FromJwk(string json)
        {
            var jwk =
                JsonSerializer.Deserialize<PrivateJwk>(json)
                ?? throw new InvalidOperationException("The SMART backend key is not a JWK");
            if (string.IsNullOrEmpty(jwk.D) || string.IsNullOrEmpty(jwk.Kid))
                throw new InvalidOperationException("The SMART backend key must be a private RSA JWK with a kid");

            var rsa = RSA.Create();
            rsa.ImportParameters(
                new RSAParameters
                {
                    Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
                    Exponent = Base64UrlEncoder.DecodeBytes(jwk.E),
                    D = Base64UrlEncoder.DecodeBytes(jwk.D),
                    P = Base64UrlEncoder.DecodeBytes(jwk.P),
                    Q = Base64UrlEncoder.DecodeBytes(jwk.Q),
                    DP = Base64UrlEncoder.DecodeBytes(jwk.Dp),
                    DQ = Base64UrlEncoder.DecodeBytes(jwk.Dq),
                    InverseQ = Base64UrlEncoder.DecodeBytes(jwk.Qi),
                }
            );
            return (rsa, jwk.Kid);
        }

        private sealed class PrivateJwk
        {
            [JsonPropertyName("kty")]
            public string Kty { get; set; } = "RSA";

            [JsonPropertyName("kid")]
            public string Kid { get; set; }

            [JsonPropertyName("n")]
            public string N { get; set; }

            [JsonPropertyName("e")]
            public string E { get; set; }

            [JsonPropertyName("d")]
            public string D { get; set; }

            [JsonPropertyName("p")]
            public string P { get; set; }

            [JsonPropertyName("q")]
            public string Q { get; set; }

            [JsonPropertyName("dp")]
            public string Dp { get; set; }

            [JsonPropertyName("dq")]
            public string Dq { get; set; }

            [JsonPropertyName("qi")]
            public string Qi { get; set; }

            public static PrivateJwk From(RSA rsa, string kid)
            {
                var p = rsa.ExportParameters(includePrivateParameters: true);
                return new PrivateJwk
                {
                    Kid = kid,
                    N = Base64UrlEncoder.Encode(p.Modulus),
                    E = Base64UrlEncoder.Encode(p.Exponent),
                    D = Base64UrlEncoder.Encode(p.D),
                    P = Base64UrlEncoder.Encode(p.P),
                    Q = Base64UrlEncoder.Encode(p.Q),
                    Dp = Base64UrlEncoder.Encode(p.DP),
                    Dq = Base64UrlEncoder.Encode(p.DQ),
                    Qi = Base64UrlEncoder.Encode(p.InverseQ),
                };
            }
        }
    }
}
