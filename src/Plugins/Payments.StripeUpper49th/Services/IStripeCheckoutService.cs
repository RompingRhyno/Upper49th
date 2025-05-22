using Grand.Domain.Orders;

namespace Payments.StripeUpper49th.Services;

public interface IStripeCheckoutService
{
    Task<string> CreateRedirectUrl(Order order);
    Task<string> CreateSubscriptionRedirectUrl(Order order, string providerCustomerId);
    Task<bool> WebHookProcessPayment(string stripeSignature, string json);
    Task<bool> WebHookProcessInvoicePaid(string stripeSignature, string json);
}