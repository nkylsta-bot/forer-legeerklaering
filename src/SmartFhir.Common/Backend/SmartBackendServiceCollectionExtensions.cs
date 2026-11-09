using Microsoft.Extensions.DependencyInjection;

namespace SmartFhir.Common.Backend
{
    public static class SmartBackendServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the SMART Backend Services key and token client. Configure
        /// SmartOnFhir:Backend:ClientId, and outside Development the private key.
        /// </summary>
        public static IServiceCollection AddSmartBackendServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<SmartBackendKeyProvider>();
            services.AddSingleton<SmartBackendServicesClient>();
            return services;
        }
    }
}
