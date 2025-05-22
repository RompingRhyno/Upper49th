using Grand.Business.Core.Commands.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Domain.Orders;
using Grand.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using Grand.Business.Core.Interfaces.Common.Configuration;
using Grand.Data;
using Customer.Membership.Data.Entities;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Domain.Payments;
using Customer.Membership.Data.Enums;

namespace Payments.StripeUpper49th.Services;

public class StripeCheckoutService : IStripeCheckoutService
{
    private readonly ILogger<StripeCheckoutService> _logger;
    private readonly IMediator _mediator;
    private readonly IPaymentTransactionService _paymentTransactionService;
    private readonly StripeCheckoutPaymentSettings _stripeCheckoutPaymentSettings;
    private readonly IContextAccessor _contextAccessor;
    private readonly ISettingService _settingService;
    private readonly IOrderService _orderService;
    private readonly IRepository<UserSubscription> _userSubscriptionRepository;

    public StripeCheckoutService(
        IContextAccessor contextAccessor,
        StripeCheckoutPaymentSettings stripeCheckoutPaymentSettings,
        ILogger<StripeCheckoutService> logger,
        IMediator mediator,
        IPaymentTransactionService paymentTransactionService,
        ISettingService settingService,
        IOrderService orderService,
        IRepository<UserSubscription> userSubscriptionRepository)
    {
        _contextAccessor = contextAccessor;
        _stripeCheckoutPaymentSettings = stripeCheckoutPaymentSettings;
        _logger = logger;
        _mediator = mediator;
        _paymentTransactionService = paymentTransactionService;
        _settingService = settingService;
        _orderService = orderService;
        _userSubscriptionRepository = userSubscriptionRepository;
    }

    public async Task<string> CreateRedirectUrl(Order order)
    {
        var session = await CreateUrlSession(order);
        return session.Url;
    }

    private async Task<Session> CreateUrlSession(Order order)
    {
        StripeConfiguration.ApiKey = _stripeCheckoutPaymentSettings.ApiKey;

        var storeLocation = _contextAccessor.StoreContext.CurrentHost.Url.TrimEnd('/');

        var options = new SessionCreateOptions {
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
            PaymentIntentData = new SessionPaymentIntentDataOptions {
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

    public async Task<string> CreateSubscriptionRedirectUrl(Order order, string providerCustomerId)
    {
        var session = await CreateSubscriptionUrlSession(order, providerCustomerId);
        return session.Url;
    }

    private async Task<Session> CreateSubscriptionUrlSession(Order order, string providerCustomerId)
    {
        // if (string.IsNullOrWhiteSpace(providerCustomerId))
        //     throw new ArgumentException("Provider customer ID cannot be null or empty.", nameof(providerCustomerId));

        StripeConfiguration.ApiKey = _stripeCheckoutPaymentSettings.ApiKey;

        var storeLocation = _contextAccessor.StoreContext.CurrentHost.Url.TrimEnd('/');
        var successUrl = $"{storeLocation}/membership/paymentsuccess?orderId={order.Id}";
        var cancelUrl = $"{storeLocation}/membership/paymentcancel?orderId={order.Id}";

        var price = await CreateRecurringPrice(order);
        var planName = order.OrderTags.FirstOrDefault() ?? "Membership Plan";

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions> {
                new SessionLineItemOptions {
                    Price = price.Id,
                    Quantity = 1
                }
            },
            ClientReferenceId = order.Id,
            CustomerEmail = order.CustomerEmail,
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "order_guid", order.OrderGuid.ToString() },
                { "customer_id", order.CustomerId },
                { "plan", planName }
            }
        };

        var service = new SessionService();
        return await service.CreateAsync(options);
    }

    // Todo: mapping the price to planids that are in joe's model
    private async Task<Price> CreateRecurringPrice(Order order)
    {
        var amountInCents = (long)(order.OrderTotal * 100);
        var currency = string.IsNullOrWhiteSpace(order.CustomerCurrencyCode)
            ? "cad"
            : order.CustomerCurrencyCode.ToLowerInvariant();

        var planName = order.OrderTags.FirstOrDefault() ?? "Membership Plan";
        var priceOptions = new PriceCreateOptions
        {
            UnitAmount = amountInCents,
            Currency = currency,
            Recurring = new PriceRecurringOptions
            {
                Interval = "month"
            },
            ProductData = new PriceProductDataOptions
            {
                Name = planName
            }          
        };

        var priceService = new PriceService();
        return await priceService.CreateAsync(priceOptions);
    }

    public async Task<bool> WebHookProcessPayment(string stripeSignature, string json)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeCheckoutPaymentSettings.WebhookEndpointSecret);
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                _logger.LogError("Invalid payment intent object in webhook");
                return false;
            }

            await CreatePaymentTransaction(paymentIntent);
            return true;
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "StripeException");
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in WebHookProcessPayment");
            return false;
        }
    }

    public async Task WebHookProcessCheckoutSessionCompleted(string stripeSignature, string json)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeCheckoutPaymentSettings.WebhookEndpointSecret);

        if (stripeEvent.Data.Object is not Session session)
            return;

        // Check if payment is complete
        if (session.PaymentStatus != "paid")
            return;

        // Retrieve metadata
        var customerId = session.Metadata["customer_id"];
        var orderGuidString = session.Metadata["order_guid"];
        var planId = session.Metadata["plan"];
        var providerCustomerId = session.Customer is Stripe.Customer c ? c.Id : session.Customer?.ToString();
        var providerSubscriptionId = session.Subscription is Stripe.Subscription s ? s.Id : session.Subscription?.ToString();


        if (!Guid.TryParse(orderGuidString, out var orderGuid))
            throw new Exception("Invalid OrderGuid metadata from Stripe session.");

        var order = await _orderService.GetOrderByGuid(orderGuid);
        if (order == null)
            throw new Exception($"Order with GUID {orderGuid} not found.");

        // Update order payment status
        order.PaymentStatusId = PaymentStatus.Paid;
        await _orderService.UpdateOrder(order);

        // Mark transaction as paid
        var transaction = await _paymentTransactionService.GetOrderByGuid(order.OrderGuid);
        if (transaction != null)
        {
            await _mediator.Send(new MarkAsPaidCommand { PaymentTransaction = transaction });
        }

        // Create UserSubscription document
        var subscription = new UserSubscription
        {
            UserId = customerId,
            PlanId = planId,
            Provider = "Payments.StripeUpper49th",
            ProviderCustomerId = providerCustomerId,
            ProviderSubscriptionId = providerSubscriptionId,
            StartDate = DateTime.UtcNow,
            RenewalDate = DateTime.UtcNow.AddMonths(1),
            Status = SubscriptionStatus.Active,
            IsActive = true,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _userSubscriptionRepository.InsertAsync(subscription);
    }

    public async Task<bool> WebHookProcessInvoicePaid(string invoiceId, string customerId)
    {
        _logger.LogInformation("Invoice paid webhook received.");
        return true;
    }

    private async Task CreatePaymentTransaction(PaymentIntent paymentIntent)
    {
        if (paymentIntent.Metadata.TryGetValue("order_guid", out var orderGuidStr) &&
            Guid.TryParse(orderGuidStr, out var orderGuid))
        {
            var paymentTransaction = await _paymentTransactionService.GetOrderByGuid(orderGuid);
            if (paymentTransaction == null)
            {
                _logger.LogWarning($"No payment transaction found for order_guid: {orderGuid}");
                return;
            }

            paymentTransaction.AuthorizationTransactionId = paymentIntent.Id;
            paymentTransaction.PaidAmount += paymentIntent.AmountReceived / 100;

            await _mediator.Send(new MarkAsPaidCommand { PaymentTransaction = paymentTransaction });

            _logger.LogInformation($"PaymentIntent succeeded and recorded for order_guid: {orderGuid}");
        }
        else
        {
            _logger.LogWarning("PaymentIntent missing or invalid order_guid metadata");
        }
    }
}
