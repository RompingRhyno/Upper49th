using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Grand.Infrastructure;
using Grand.Web.Common.Controllers;
using Grand.Web.Common;
using Grand.Business.Common.Services.Directory;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Business.Customers.Services;
using Customer.Membership.Models;
using Grand.Business.Core.Interfaces.Common.Configuration;
using Customer.Membership.Domain.Settings;
using Customer.Membership.Domain;
using Grand.Web.Models.Checkout;
using PaymentMethodModel = Grand.Web.Models.Checkout.CheckoutPaymentMethodModel.PaymentMethodModel;
using MediatR;
using Grand.Domain.Common;
using Grand.Web.Features.Models.Checkout;
using Grand.Domain.Orders;
using Grand.Business.Core.Commands.Checkout.Orders;
using Grand.Business.Core.Enums.Checkout;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Business.Core.Utilities.Checkout;
using Grand.Domain.Customers;
using Grand.Domain.Payments;
using Grand.Web.Models.Common;
using Grand.Domain.Directory;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payments.StripeCheckout.Services;

namespace Customer.Membership.Controllers
{
    [Route("membership")]
    public class MembershipController : Controller
    {
        private readonly ILogger<MembershipController> _logger;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGroupService _groupService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly IMediator _mediator;
        private readonly AddressSettings _addressSettings;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly OrderSettings _orderSettings;
        private readonly ICountryService _countryService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IStripeCheckoutService _stripeCheckoutService;




        public MembershipController(
            ILogger<MembershipController> logger,
            IContextAccessor contextAccessor,
            IGroupService groupService,
            ICustomerService customerService,
            ISettingService settingService,
            IMediator mediator,
            AddressSettings addressSettings,
            IPaymentService paymentService,
            IOrderService orderService,
            OrderSettings orderSettings,
            ICountryService countryService,
            IPaymentTransactionService paymentTransactionService,
            IStripeCheckoutService stripeCheckoutService)
        {
            _logger = logger;
            _workContext = contextAccessor.WorkContext;
            _storeContext = contextAccessor.StoreContext;
            _groupService = groupService;
            _customerService = customerService;
            _settingService = settingService;
            _mediator = mediator;
            _addressSettings = addressSettings;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderSettings = orderSettings;
            _countryService = countryService;
            _paymentTransactionService = paymentTransactionService;
            _stripeCheckoutService = stripeCheckoutService;
        }

        [HttpGet("", Name = "MembershipIndex")]
        public IActionResult Index()
        {
            return View("~/Views/Membership/Index.cshtml");
        }

        [HttpGet("signup")]
        public async Task<IActionResult> SignUp()
        {
            await _customerService.ResetCheckoutData(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            var model = new PlanSelectionModel
            {
                AvailablePlans = await LoadAvailablePlansAsync()
            };

            return View("~/Views/Membership/SignUp.cshtml", model);
        }

        [HttpPost("signup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(MembershipWizardModel model, string navigation)
        {
            // Back navigation
            if (navigation == "back")
            {
                var previousStep = Math.Max(1, model.CurrentStep - 1);

                if (previousStep == 1)
                {
                    var planModel = new PlanSelectionModel
                    {
                        CurrentStep = 1,
                        SelectedPlan = model.SelectedPlan,
                        AvailablePlans = await LoadAvailablePlansAsync()
                    };

                    return View("~/Views/Membership/SignUp.cshtml", planModel);
                }
                else if (previousStep == 2)
                {
                    var billingModel = new BillingAddressModel
                    {
                        CurrentStep = 2,
                        SelectedPlan = model.SelectedPlan,
                        BillingAddress = GetCustomerBillingAddressModel()
                    };

                    billingModel.BillingAddress.AvailableCountries = (await _countryService.GetAllCountries()).Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id
                    }).ToList();

                    return View("~/Views/Membership/SignUp.cshtml", billingModel);
                }
            }

            //Step 1: Plan Selection
            if (model.CurrentStep == 1)
            {

                var planModel = new PlanSelectionModel();

                if (!await TryUpdateModelAsync(planModel))
                {
                    _logger.LogWarning("TryUpdateModelAsync failed for PlanSelectionModel");
                }

                //Check if model is invalid and if it is reload page
                if (!ModelState.IsValid)
                {
                    planModel.AvailablePlans = await LoadAvailablePlansAsync();
                    return View("~/Views/Membership/SignUp.cshtml", planModel);
                }

                if (string.IsNullOrWhiteSpace(planModel.SelectedPlan))
                {
                    ModelState.AddModelError(nameof(planModel.SelectedPlan), "Please select a plan.");
                    planModel.AvailablePlans = await LoadAvailablePlansAsync();
                    return View("~/Views/Membership/SignUp.cshtml", planModel);
                }

                //Move to step 2
                var billingModel = new BillingAddressModel
                {
                    CurrentStep = 2,
                    SelectedPlan = planModel.SelectedPlan,
                    BillingAddress = GetCustomerBillingAddressModel(),
                };

                billingModel.BillingAddress.AvailableCountries = (await _countryService.GetAllCountries()).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id
                }).ToList();

                return View("~/Views/Membership/SignUp.cshtml", billingModel);
            }
            // Step 2: Billing Address
            else if (model.CurrentStep == 2)
            {
                var billingModel = new BillingAddressModel();
                billingModel.CurrentStep = model.CurrentStep;
                billingModel.SelectedPlan = model.SelectedPlan;

                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation($"Form key: {key}, Value: {Request.Form[key]}");
                }

                if (!await TryUpdateModelAsync(billingModel.BillingAddress, prefix: "BillingAddress"))
                {
                    _logger.LogWarning("TryUpdateModelAsync failed for BillingModel");
                    billingModel.BillingAddress.AvailableCountries = (await _countryService.GetAllCountries()).Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id
                    }).ToList();

                    return View("~/Views/Membership/SignUp.cshtml", billingModel);
                }

                if (!ModelState.IsValid)
                {
                    billingModel.CurrentStep = 2;
                    billingModel.SelectedPlan = model.SelectedPlan;
                    billingModel.BillingAddress.AvailableCountries = (await _countryService.GetAllCountries()).Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id
                    }).ToList();

                    return View("~/Views/Membership/SignUp.cshtml", billingModel);
                }

                // TODO: Add validation to make sure all fields are filled out before proceeding and modifying database with blanks

                var addressModel = billingModel.BillingAddress;
                var address = new Address
                {
                    FirstName = addressModel.FirstName,
                    LastName = addressModel.LastName,
                    Email = addressModel.Email,
                    CountryId = addressModel.CountryId,
                    StateProvinceId = addressModel.StateProvinceId,
                    City = addressModel.City,
                    Address1 = addressModel.Address1,
                    ZipPostalCode = addressModel.ZipPostalCode,
                    PhoneNumber = addressModel.PhoneNumber
                };

                _workContext.CurrentCustomer.BillingAddress = address;
                await _customerService.UpdateBillingAddress(address, _workContext.CurrentCustomer.Id);

                // Move to step 3
                var paymentSelectionModel = new PaymentMethodSelectionModel()
                {
                    CurrentStep = 3,
                    SelectedPlan = model.SelectedPlan,
                    PaymentMethods = await LoadAvailablePaymentMethods()
                };

                return View("~/Views/Membership/SignUp.cshtml", paymentSelectionModel);
            }
            // Step 3
            else if (model.CurrentStep == 3)
            {
                var paymentModel = new PaymentMethodSelectionModel
                {
                    CurrentStep = model.CurrentStep,
                    SelectedPlan = model.SelectedPlan,
                    PaymentMethods = await LoadAvailablePaymentMethods()
                };

                if (!await TryUpdateModelAsync(paymentModel))
                {
                    _logger.LogWarning("TryUpdateModelAsync failed for PaymentMethodSelectionModel");
                }

                _logger.LogWarning(paymentModel.SelectedPaymentMethod);

                if (string.IsNullOrEmpty(paymentModel.SelectedPaymentMethod))
                {
                    ModelState.AddModelError(nameof(paymentModel.SelectedPaymentMethod), "Please select a payment method.");
                }

                if (!ModelState.IsValid)
                {
                    paymentModel.PaymentMethods = await LoadAvailablePaymentMethods();
                    return View("~/Views/Membership/SignUp.cshtml", paymentModel);
                }

                // Move to step 4
                var paymentProcessModel = new PaymentProcessModel()
                {
                    CurrentStep = 4,
                    SelectedPlan = model.SelectedPlan,
                    SelectedPaymentMethod = paymentModel.SelectedPaymentMethod
                };

                var paymentMethodSystemName = paymentModel.SelectedPaymentMethod;

                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);

                if (paymentMethod != null)
                {
                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {

                        var newOrder = new Order
                        {
                            OrderNumber = GenerateOrderNumber(),
                            OrderTotal = (double)(await GetPlanAmount(model.SelectedPlan)),
                            CustomerCurrencyCode = _workContext.WorkingCurrency?.CurrencyCode,
                            CustomerEmail = _workContext.CurrentCustomer.Email,
                            OrderGuid = Guid.NewGuid(),
                            CustomerId = _workContext.CurrentCustomer.Id,
                            CreatedOnUtc = DateTime.UtcNow
                        };

                        newOrder.OrderTags.Add(model.SelectedPlan);

                        await _orderService.InsertOrder(newOrder);


                        var paymentTransaction = new PaymentTransaction
                        {
                            StoreId = _storeContext.CurrentStore.Id,
                            CustomerId = _workContext.CurrentCustomer.Id,
                            PaymentMethodSystemName = paymentMethodSystemName,
                            TransactionAmount = (double)(await GetPlanAmount(model.SelectedPlan)),
                            CurrencyCode = _workContext.WorkingCurrency.CurrencyCode,
                            TransactionStatus = TransactionStatus.Pending

                        };

                        await _paymentTransactionService.InsertPaymentTransaction(paymentTransaction);

                        // Get redirect url
                        var redirectUrl = await _stripeCheckoutService.CreateRedirectUrl(newOrder);
                        _logger.LogWarning(redirectUrl);

                        if (!string.IsNullOrEmpty(redirectUrl))
                        {
                            paymentProcessModel.RedirectUrl = redirectUrl;
                        }
                        // For non-redirect methods, get the view component name (CURRENTLY UNUSED)
                        else
                        {
                            // paymentProcessModel.PaymentViewComponent = "PaymentBrainTree"; //paymentMethod.GetPublicViewComponentName();

                            // paymentProcessModel.PaymentAdditionalData = new Dictionary<string, string>
                            // {
                            //     { "amount", (await GetPlanAmount(model.SelectedPlan)).ToString() },
                            //     { "currency", _workContext.WorkingCurrency.CurrencyCode }
                            // };
                        }
                        return View("~/Views/Membership/SignUp.cshtml", paymentProcessModel);
                    }
                }
            }
            return View("~/Views/Membership/Index.cshtml");
        }

        [HttpGet("paymentsuccess")]
        public async Task<IActionResult> PaymentSuccess(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return BadRequest("Missing order ID.");

            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                return NotFound("Order not found.");

            var selectedPlan = order.OrderTags?.FirstOrDefault() ?? string.Empty;

            var memberGroup = await _groupService.GetCustomerGroupBySystemName(selectedPlan);
            await _customerService.InsertCustomerGroupInCustomer(memberGroup, _workContext.CurrentCustomer.Id);

            var model = new OrderViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Total = order.OrderTotal,
                Email = order.CustomerEmail,
                Currency = order.CustomerCurrencyCode,
                MembershipRole = await GetPlanRole(selectedPlan)
            };

            return View("PaymentSuccess", model);
        }

        [HttpGet("paymentcancel")]
        public async Task<IActionResult> PaymentCancel(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return BadRequest("Missing order ID.");

            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                return NotFound("Order not found.");

            // TODO: UPDATE ORDER STATUS HERE

            var model = new OrderViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Total = order.OrderTotal,
                Email = order.CustomerEmail,
                Currency = order.CustomerCurrencyCode
            };

            return View("PaymentCancel", model);
        }


        [HttpGet("membershipinfo")]
        public async Task<IActionResult> MembershipInfo()
        {
            var model = new UserSubscription()
            {
                UserId = "abc123",
                PlanId = "premium-plan",
                Provider = "Stripe",
                ProviderCustomerId = "cus_789",
                ProviderSubscriptionId = "sub_456",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                RenewalDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true
            };
            return View("MembershipInfo", model);
        }


        // Helper function region
        // Get customer's existing billing address
        private AddressModel GetCustomerBillingAddressModel()
        {
            var billingAddress = _workContext.CurrentCustomer.BillingAddress;

            if (billingAddress == null)
                return new AddressModel();

            return new AddressModel
            {
                FirstName = billingAddress.FirstName,
                LastName = billingAddress.LastName,
                Email = billingAddress.Email,
                CountryId = billingAddress.CountryId,
                StateProvinceId = billingAddress.StateProvinceId,
                City = billingAddress.City,
                Address1 = billingAddress.Address1,
                Address2 = billingAddress.Address2,
                ZipPostalCode = billingAddress.ZipPostalCode,
                PhoneNumber = billingAddress.PhoneNumber
            };
        }

        // Helper function for loading all available membership plans
        private async Task<List<MembershipPlan>> LoadAvailablePlansAsync()
        {
            var settings = await _settingService.LoadSetting<MembershipSettings>();
            return settings.Plans.Select(p => new MembershipPlan
            {
                Role = p.Role,
                Price = p.Price,
                SystemName = p.SystemName
            }).ToList();
        }

        // Helper function to get payment methods
        private async Task<List<CheckoutPaymentMethodModel.PaymentMethodModel>> LoadAvailablePaymentMethods()
        {
            var customer = _workContext.CurrentCustomer;
            var store = _storeContext.CurrentStore;
            var language = _workContext.WorkingLanguage;
            var currency = _workContext.WorkingCurrency;

            var filterByCountryId = "";
            if (_addressSettings.CountryEnabled &&
                customer.BillingAddress != null &&
                !string.IsNullOrEmpty(customer.BillingAddress.CountryId))
            {
                filterByCountryId = customer.BillingAddress.CountryId;
            }

            var model = await _mediator.Send(new GetPaymentMethod
            {
                // Not needed, can leave empty
                Cart = new List<ShoppingCartItem>(),
                Customer = customer,
                Currency = currency,
                Language = language,
                Store = store,
                FilterByCountryId = filterByCountryId
            });

            return model.PaymentMethods.ToList();
        }

        // Helper function to get plan role string
        private async Task<string> GetPlanRole(string selectedPlan)
        {
            var availablePlans = await LoadAvailablePlansAsync();
            var plan = availablePlans.FirstOrDefault(p => p.SystemName == selectedPlan);

            if (plan == null)
            {
                throw new Exception($"Plan '{selectedPlan}' not found");
            }

            return plan.Role;
        }

        // Helper function to get plan price
        private async Task<decimal> GetPlanAmount(string selectedPlan)
        {
            var availablePlans = await LoadAvailablePlansAsync();
            var plan = availablePlans.FirstOrDefault(p => p.SystemName == selectedPlan);

            if (plan == null)
            {
                throw new Exception($"Plan '{selectedPlan}' not found");
            }

            return plan.Price;
        }

        // Helper function to generate a random Order number
        private int GenerateOrderNumber()
        {
            var ticks = DateTime.UtcNow.Ticks;
            return (int)(ticks % int.MaxValue);
        }
    }
}