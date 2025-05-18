using Grand.Business.Core.Commands.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Domain.Orders;
using Grand.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Payments.StripeUpper49th.Services;

public class StripeCheckoutService : IStripeCheckoutService
{
    private readonly ILogger<StripeCheckoutService> _logger;
    private readonly IMediator _mediator;
    private readonly IPaymentTransactionService _paymentTransactionService;
    private readonly StripeCheckoutPaymentSettings _stripeCheckoutPaymentSettings;
    private readonly IContextAccessor _contextAccessor;

    public StripeCheckoutService(
        IContextAccessor contextAccessor,
        StripeCheckoutPaymentSettings stripeCheckoutPaymentSettings,
        ILogger<StripeCheckoutService> logger,
        IMediator mediator,
        IPaymentTransactionService paymentTransactionService)
    {
        _contextAccessor = contextAccessor;
        _stripeCheckoutPaymentSettings = stripeCheckoutPaymentSettings;
        _logger = logger;
        _mediator = mediator;
        _paymentTransactionService = paymentTransactionService;
    }

    public async Task<string> CreateRedirectUrl(Order order)
    {
        var session = await CreateUrlSession(order);
        return session.Url;
    }

    public async Task<string> CreateSubscriptionRedirectUrl(string userEmail, string priceId)
    {
        var session = await CreateSubscriptionSession(userEmail, priceId);
        return session.Url;
    }

    public async Task<bool> WebHookProcessPayment(string stripeSignature, string json)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                _stripeCheckoutPaymentSettings.WebhookEndpointSecret
            );

            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await CreatePaymentTransaction(paymentIntent);
                    return true;

                case Events.CheckoutSessionCompleted:
                    var session = stripeEvent.Data.Object as Session;

                    if (session.Mode == "subscription" && !string.IsNullOrEmpty(session.Subscription))
                    {
                        _logger.LogInformation("Subscription session completed: {SessionId}, Customer: {CustomerId}, Subscription: {SubscriptionId}",
                            session.Id, session.CustomerId, session.Subscription);

                        // store Stripe Customer ID and Subscription ID
                        await SaveSubscriptionDetails(session);

                        // assign role, send welcome email, etc.
                    }
                    return true;
            }
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "StripeException");
            return false;
        }

        return false;
    }

    private async Task CreatePaymentTransaction(PaymentIntent paymentIntent)
    {
        if (paymentIntent.Metadata.TryGetValue("order_guid", out var order_guid)
            && Guid.TryParse(order_guid, out var orderGuid))
        {
            var paymentTransaction = await _paymentTransactionService.GetOrderByGuid(orderGuid);
            if (paymentTransaction == null ||
                !paymentIntent.Currency.Equals(paymentTransaction.CurrencyCode,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogError("paymentTransaction is null or currency is not equal");
                return;
            }

            try
            {
                paymentTransaction.AuthorizationTransactionId = paymentIntent.Id;
                paymentTransaction.PaidAmount += paymentIntent.Amount / 100;
                await _mediator.Send(new MarkAsPaidCommand { PaymentTransaction = paymentTransaction });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in CreatePaymentTransaction");
            }
        }
        else
        {
            _logger.LogError("order_guid can't get from metadata or orderGuid is not valid");
        }
    }

    private async Task<Session> CreateUrlSession(Order order)
    {
        StripeConfiguration.ApiKey = _stripeCheckoutPaymentSettings.ApiKey;

        var storeLocation = _contextAccessor.StoreContext.CurrentHost.Url.TrimEnd('/');

        var options = new SessionCreateOptions
        {
            LineItems = [
                new SessionLineItemOptions {
                    PriceData = new SessionLineItemPriceDataOptions {
                        UnitAmountDecimal = (decimal?)order.OrderTotal * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions {
                            Name = string.Format(_stripeCheckoutPaymentSettings.Line, order.OrderNumber)
                        },
                        Currency = order.CustomerCurrencyCode
                    },
                    Quantity = 1
                }
            ],
            ClientReferenceId = order.Id,
            CustomerEmail = order.CustomerEmail,
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string> { { "order_guid", order.OrderGuid.ToString() } }
            },
            Mode = "payment",
            SuccessUrl = $"{storeLocation}/orderdetails/{order.Id}",
            CancelUrl = $"{storeLocation}/Plugins/PaymentStripeCheckout/CancelOrder/{order.Id}"
        };
        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return session;
    }
    
    private async Task<Session> CreateSubscriptionSession(string userEmail, string priceId)
    {
        StripeConfiguration.ApiKey = _stripeCheckoutPaymentSettings.ApiKey;

        var storeLocation = _contextAccessor.StoreContext.CurrentHost.Url.TrimEnd('/');

        var options = new SessionCreateOptions
        {
            CustomerEmail = userEmail,
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            },
            SuccessUrl = $"{storeLocation}/Plugins/PaymentStripeCheckout/SubscriptionSuccess",
            CancelUrl = $"{storeLocation}/Plugins/PaymentStripeCheckout/SubscriptionCancel"
        };

        var service = new SessionService();
        return await service.CreateAsync(options);
    }

}