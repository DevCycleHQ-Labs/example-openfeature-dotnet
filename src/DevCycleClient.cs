using DevCycle.SDK.Server.Common.Model.Local;
using DevCycle.SDK.Server.Local.Api;
using OpenFeature;
using OpenFeature.Model;
using Environment = System.Environment;

namespace HelloTogglebot
{
    public class DevCycleClient
    {
        private static DevCycleLocalClient? client;
        private static FeatureClient? ofClient;
        private static bool initialized;

        public static async Task Initialize()
        {
            var DEVCYCLE_SDK_KEY = Environment.GetEnvironmentVariable("DEVCYCLE_SERVER_SDK_KEY");
            if (DEVCYCLE_SDK_KEY == null)
            {
                Console.WriteLine("DEVCYCLE_SERVER_SDK_KEY environment variable not set");
                return;
            }

            // Initialize the DevCycle SDK client
            client = new DevCycleLocalClientBuilder()
                .SetSDKKey(DEVCYCLE_SDK_KEY)
                .SetOptions(
                    new DevCycleLocalOptions(configPollingIntervalMs: 5000, eventFlushIntervalMs: 1000)
                )
                .SetInitializedSubscriber((o, e) =>
                {
                    if (e.Success)
                    {
                        initialized = true;
                    }
                    else
                    {
                        Console.WriteLine($"DevCycle Client did not initialize. Errors: {e.Errors}");
                    }
                })
                .Build();


            await Api.Instance.SetProviderAsync(client.GetOpenFeatureProvider());
            ofClient  = Api.Instance.GetClient();
            ofClient.SetContext(
                EvaluationContext.Builder().Set("user_id", "api-user").Build());
            
            try
            {
                await Task.Delay(5000);
            }
            catch (TaskCanceledException)
            {
                Environment.Exit(0);
            }
        }

        public static DevCycleLocalClient GetClient()
        {
            if (!initialized || client == null)
            {
                throw new Exception("DevCycle Client not initialized");
            }

            return client;
        }

        public static FeatureClient GetOpenFeatureClient()
        {
            if (ofClient != null) return ofClient;
            throw new Exception("DevCycle Client/OpenFeature client not initialized");
        }
    }
}