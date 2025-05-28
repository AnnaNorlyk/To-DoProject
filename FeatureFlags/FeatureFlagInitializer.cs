using FeatureHubSDK;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags
{
    public static class FeatureFlagInitializer
    {
        public static void Configure(IServiceCollection services)
        {
            var edgeUrl = Environment.GetEnvironmentVariable("FEATUREHUB_URL");
            var apiKey = Environment.GetEnvironmentVariable("FEATUREHUB_API_KEY");

            Console.WriteLine($"FeatureHub URL: {edgeUrl}");
            Console.WriteLine($"FeatureHub API Key: {apiKey}");

            if (!string.IsNullOrWhiteSpace(edgeUrl) && !string.IsNullOrWhiteSpace(apiKey))
            {
                try
                {
                    var featureHubContext = new EdgeFeatureHubConfig(edgeUrl, apiKey)
                        .NewContext()
                        .Build()
                        .GetAwaiter()
                        .GetResult();

                    services.AddSingleton<IClientContext>(featureHubContext);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to connect to FeatureHub: {e.Message}");
                }
            }

            // Fallback to null context to prevent crashes
            services.AddSingleton<IClientContext>(_ => null!);
        }
    }
}
