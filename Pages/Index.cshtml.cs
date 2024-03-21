using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DevCycle.SDK.Server.Local.Api;
using DevCycle.SDK.Server.Common.Model;
using Google.Protobuf.WellKnownTypes;
using OpenFeature.Model;

namespace HelloTogglebot.Pages;

public class IndexModel : PageModel
{
    public string Speed { get; private set; } = "off";
    public bool Wink { get; private set; } = false;

    public string Message { get; private set; } = "";
    public string VariationName { get; private set; } = "Default";
    public string TogglebotSrc { get; private set; } = "";

    public string Header { get; private set; } = "";
    public string Body { get; private set; } = "";

    public IndexModel()
    {
    }

    public async void OnGet()
    {
        // Get the user defined on the request context
        DevCycleUser? user = (DevCycleUser?)HttpContext.Items["user"];
        EvaluationContext? context = (EvaluationContext?)HttpContext.Items["ofContext"];
        if (user == null || context == null)
        {
            throw new Exception("User not defined in request context");
        }

        DevCycleLocalClient client = DevCycleClient.GetClient();
        var ofClient = DevCycleClient.GetOpenFeatureClient();
        Dictionary<string, Feature> features = await client.AllFeatures(user);
        VariationName = features.TryGetValue("hello-togglebot", out var feature)
            ? feature.VariationName
            : "Default";

        Wink = await ofClient.GetBooleanValue("togglebot-wink", false, context);
        Speed = await ofClient.GetStringValue("togglebot-speed", "off", context);

        Message = Speed switch
        {
            "slow" => "Awesome, look at you go!",
            "fast" => "This is fun!",
            "off-axis" => "...I'm gonna be sick...",
            "surprise" => "What the unicorn?",
            _ => "Hello! Nice to meet you."
        };

        TogglebotSrc = Wink ? "/images/togglebot-wink.svg" : "/images/togglebot.svg";
        if (Speed == "surprise")
        {
            TogglebotSrc = "/images/unicorn.svg";
        }

        string step = await ofClient.GetStringValue("example-text", "default", context);

        switch (step)
        {
            case "step-1":
                Header = "Welcome to DevCycle's example app.";
                Body =
                    "If you got here through the onboarding flow, just follow the instructions to change and create new Variations and see how the app reacts to new Variable values.";
                break;
            case "step-2":
                Header = "Great! You've taken the first step in exploring DevCycle.";
                Body =
                    "You've successfully toggled your very first Variation. You are now serving a different value to your users and you can see how the example app has reacted to this change. Next, go ahead and create a whole new Variation to see what else is possible in this app.";
                break;
            case "step-3":
                Header = "You're getting the hang of things.";
                Body =
                    "By creating a new Variation with new Variable values and toggling it on for all users, you've already explored the fundamental concepts within DevCycle. There's still so much more to the platform, so go ahead and complete the onboarding flow and play around with the feature that controls this example in your dashboard.";
                break;
            default:
                Header = "Welcome to DevCycle's example app.";
                Body =
                    "If you got to the example app on your own, follow our README guide to create the Feature and Variables you need to control this app in DevCycle.";
                break;
        }
    }
}