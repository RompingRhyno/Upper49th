using Grand.Domain.Orders;

namespace Payments.StripeUpper49th.Services;

public interface IStripeCheckoutService
{
    Task<string> CreateRedirectUrl(Order order);
    Task<bool> WebHookProcessPayment(string stripeSignature, string json);
}