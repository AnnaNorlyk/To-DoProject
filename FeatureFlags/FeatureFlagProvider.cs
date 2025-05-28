using FeatureHubSDK;

namespace FeatureFlags
{
    public interface IFeatureFlagProvider
    {
        IClientContext Context { get; }
    }

    public class FeatureFlagProvider : IFeatureFlagProvider
    {
        public IClientContext Context { get; private set; }

        public FeatureFlagProvider(string edgeUrl, string apiKey)
        {
            var config = new EdgeFeatureHubConfig(edgeUrl, apiKey);
            Context = config.NewContext().Build().GetAwaiter().GetResult();
        }
    }
}
