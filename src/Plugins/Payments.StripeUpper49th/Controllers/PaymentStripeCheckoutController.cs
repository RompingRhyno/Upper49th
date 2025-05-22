using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Domain.Payments;
using Grand.Infrastructure;
using Grand.Web.Common.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Payments.StripeUpper49th.Models;
using Payments.StripeUpper49th.Services;
using Stripe;
using Stripe.Checkout;

namespace Payments.StripeUpper49th.Controllers;

public class PaymentStripeCheckoutController : BasePaymentController
{
    private readonly ILogger<PaymentStripeCheckoutController> _logger;
    private readonly IOrderService _orderService;

    private readonly PaymentSettings _paymentSettings;
    private readonly IPaymentTransactionService _paymentTransactionService;
    private readonly StripeCheckoutPaymentSettings _stripeCheckoutPaymentSettings;
    private readonly IStripeCheckoutService _stripeCheckoutService;
    private readonly IContextAccessor _contextAccessor;

    public PaymentStripeCheckoutController(
        IContextAccessor contextAccessor,
        IOrderService orderService,
        ILogger<PaymentStripeCheckoutController> logger,
        IPaymentTransactionService paymentTransactionService,
        PaymentSettings paymentSettings,
        StripeCheckoutPaymentSettings stripeCheckoutPaymentSettings, IStripeCheckoutService stripeCheckoutService)
    {
        _contextAccessor = contextAccessor;
        _orderService = orderService;
        _logger = logger;
        _paymentTransactionService = paymentTransactionService;
        _paymentSettings = paymentSettings;
        _stripeCheckoutPaymentSettings = stripeCheckoutPaymentSettings;
        _stripeCheckoutService = stripeCheckoutService;
    }

    [HttpPost]
    public async Task<IActionResult> WebHook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeCheckoutPaymentSettings.WebhookEndpointSecret);

            switch (stripeEvent.Type)
            {
                case EventTypes.PaymentIntentSucceeded:
                    await _stripeCheckoutService.WebHookProcessPayment(stripeSignature, json);
                    break;
                case EventTypes.InvoicePaid:
                    await _stripeCheckoutService.WebHookProcessInvoicePaid(stripeSignature, json);
                    break;
                // can add more events as needed
                default:
                    _logger.LogInformation($"Unhandled Stripe event type: {stripeEvent.Type}");
                    break;
            }

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Webhook error");
            return BadRequest(e.Message);
        }
    }

    public async Task<IActionResult> CancelOrder(string orderId)
    {
        var order = await _orderService.GetOrderById(orderId);
        if (order != null && order.CustomerId == _contextAccessor.WorkContext.CurrentCustomer.Id)
            return RedirectToRoute("OrderDetails", new { orderId = order.Id });

        return RedirectToRoute("HomePage");
    }

    public IActionResult PaymentInfo()
    {
        return View(new PaymentInfo(_stripeCheckoutPaymentSettings.Description));
    }
}