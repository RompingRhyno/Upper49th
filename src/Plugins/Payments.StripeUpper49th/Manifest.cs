using Grand.Infrastructure.Plugins;
using Payments.StripeUpper49th;

[assembly: PluginInfo(
    FriendlyName = "Stripe Checkout for Upper49th",
    Group = "Payment methods",
    SystemName = StripeCheckoutDefaults.ProviderSystemName,
    Author = "Bit By Bit",
    Version = "1.0.0"
)]