using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SmartFhir.Common.Backend
{
    /// <summary>
    /// Gets a system access token with SMART Backend Services: the client_credentials grant, with
    /// the client authenticated by a client assertion signed with <see cref="SmartBackendKeyProvider"/>.
    ///
    /// Used where there is no clinician session, such as an Altinn service task, which runs in the
    /// workflow engine's callback and not in the clinician's browser request.
    /// </summary>
    public sealed class SmartBackendServicesClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SmartBackendKeyProvider _keyProvider;
        private readonly IConfiguration _config;

        public SmartBackendServicesClient(
            IHttpClientFactory httpClientFactory,
            SmartBackendKeyProvider keyProvider,
            IConfiguration config
        )
        {
            _httpClientFactory = httpClientFactory;
            _keyProvider = keyProvider;
            _config = config;
        }

        public async Task<string> GetSystemAccessTokenAsync(
            string fhirBaseUrl,
            IEnumerable<string> scopes,
            CancellationToken cancellationToken = default
        )
        {
            var clientId =
                _config["SmartOnFhir:Backend:ClientId"]
                ?? throw new InvalidOperationException("SmartOnFhir:Backend:ClientId is not set");
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = await DiscoverTokenEndpoint(client, fhirBaseUrl, cancellationToken);

            var now = DateTime.UtcNow;
            var assertion = new JsonWebTokenHandler().CreateToken(
                new SecurityTokenDescriptor
                {
                    Issuer = clientId,
                    Audience = tokenEndpoint,
                    Claims = new Dictionary<string, object>
                    {
                        ["sub"] = clientId,
                        ["jti"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)),
                    },
                    IssuedAt = now,
                    NotBefore = now,
                    Expires = now.AddMinutes(2),
                    SigningCredentials = _keyProvider.SigningCredentials,
                    TokenType = "JWT",
                }
            );

            using var response = await client.PostAsync(
                tokenEndpoint,
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials",
                        ["scope"] = string.Join(" ", scopes),
                        ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                        ["client_assertion"] = assertion,
                    }
                ),
                cancellationToken
            );
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new SmartBackendException(
                    $"Token request failed: {(int)response.StatusCode} {body}",
                    response.StatusCode
                );

            using var doc = JsonDocument.Parse(body);
            return
                doc.RootElement.TryGetProperty("access_token", out var token) && token.GetString() is { Length: > 0 } t
                ? t
                : throw new SmartBackendException("Token response has no access_token", response.StatusCode);
        }

        private static async Task<string> DiscoverTokenEndpoint(
            HttpClient client,
            string fhirBaseUrl,
            CancellationToken cancellationToken
        )
        {
            using var response = await client.GetAsync(
                $"{fhirBaseUrl.TrimEnd('/')}/.well-known/smart-configuration",
                cancellationToken
            );
            if (!response.IsSuccessStatusCode)
                throw new SmartBackendException(
                    $"SMART discovery failed: {(int)response.StatusCode}",
                    response.StatusCode
                );
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return
                doc.RootElement.TryGetProperty("token_endpoint", out var endpoint)
                && endpoint.GetString() is { Length: > 0 } e
                ? e
                : throw new SmartBackendException("SMART configuration has no token_endpoint", response.StatusCode);
        }
    }

    /// <summary>A failed call to the EHR. Server errors and timeouts are worth retrying, client errors are not.</summary>
    public sealed class SmartBackendException : Exception
    {
        public SmartBackendException(string message, HttpStatusCode? statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode? StatusCode { get; }

        public bool IsTransient =>
            StatusCode is null || (int)StatusCode >= 500 || StatusCode == HttpStatusCode.TooManyRequests;
    }
}
