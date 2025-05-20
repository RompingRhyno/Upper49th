using Grand.Domain.Orders;

namespace Payments.StripeCheckout.Services;

public interface IStripeCheckoutService
{
    Task<string> CreateRedirectUrl(Order order);
    Task<string> CreateRedirectUrl(Order order, string paymentType);
    Task<bool> WebHookProcessPayment(string stripeSignature, string json);
}