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
            ICountryService countryService)
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
            // Portion for back navigation
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

            _logger.LogWarning("Is model PlanSelectionModel? {IsPlanModel}", model is PlanSelectionModel);
            //Step 1: Plan Selection
            if (model.CurrentStep == 1)
            {

                _logger.LogWarning("I get here");

                var planModel = new PlanSelectionModel();

                if (!await TryUpdateModelAsync(planModel))
                {
                    _logger.LogWarning("TryUpdateModelAsync failed for PlanSelectionModel");
                }

                _logger.LogWarning("Selected plan: {SelectedPlan}", planModel.SelectedPlan);


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

                _logger.LogWarning("I get here billing");

                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation($"Form key: {key}, Value: {Request.Form[key]}");
                }

                _logger.LogWarning("I get here billing 2");

                if (!await TryUpdateModelAsync(billingModel.BillingAddress, prefix: "BillingAddress"))
                {
                    _logger.LogWarning("TryUpdateModelAsync failed for BillingModel");

                    // Log ModelState errors
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        foreach (var error in errors)
                        {
                            _logger.LogWarning($"ModelState Error for {key}: {error.ErrorMessage}");
                        }
                    }

                    // Load dropdown values only — DO NOT replace user input
                    billingModel.BillingAddress.AvailableCountries = (await _countryService.GetAllCountries()).Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id
                    }).ToList();

                    return View("~/Views/Membership/SignUp.cshtml", billingModel);
                }

                _logger.LogWarning("I get here billing 3");

                //Check if model is invalid and if it is reload pagea
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

                //Add validation to make sure all fields are filled out

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

                //Move to step 3
                var paymentSelectionModel = new PaymentMethodSelectionModel()
                {
                    CurrentStep = 3,
                    SelectedPlan = model.SelectedPlan,
                    BillingAddress = billingModel.BillingAddress,
                    PaymentMethods = await LoadAvailablePaymentMethods()
                };

                return View("~/Views/Membership/SignUp.cshtml", paymentSelectionModel);
            }
            else if (model.CurrentStep == 3)
            {
                var paymentModel = new PaymentMethodSelectionModel
                {
                    CurrentStep = model.CurrentStep,
                    SelectedPlan = model.SelectedPlan,
                    BillingAddress = GetCustomerBillingAddressModel(),
                    PaymentMethods = await LoadAvailablePaymentMethods()
                };

                if (!await TryUpdateModelAsync(paymentModel))
                {
                    _logger.LogWarning("TryUpdateModelAsync failed for PaymentMethodSelectionModel");
                }

                if (string.IsNullOrEmpty(paymentModel.SelectedPaymentMethod))
                {
                    ModelState.AddModelError(nameof(paymentModel.SelectedPaymentMethod), "Please select a payment method.");
                }

                if (!ModelState.IsValid)
                {
                    paymentModel.PaymentMethods = await LoadAvailablePaymentMethods(); // Reload options
                    return View("~/Views/Membership/SignUp.cshtml", paymentModel);
                }
            }
            return View("~/Views/Membership/Index.cshtml");
        }

        //     // Step 2: Billing Address
        //     if (model.CurrentStep == 2)
        //     {
        //         if (!ModelState.IsValid)
        //         {
        //             return View("~/Views/Membership/SignUp.cshtml", model);
        //         }

        //         // With this manual conversion:
        //         var addressModel = model.BillingAddress.BillingNewAddress;
        //         var address = new Address
        //         {
        //             FirstName = addressModel.FirstName,
        //             LastName = addressModel.LastName,
        //             Email = addressModel.Email,
        //             CountryId = addressModel.CountryId,
        //             StateProvinceId = addressModel.StateProvinceId,
        //             City = addressModel.City,
        //             Address1 = addressModel.Address1,
        //             Address2 = addressModel.Address2,
        //             ZipPostalCode = addressModel.ZipPostalCode,
        //             PhoneNumber = addressModel.PhoneNumber
        //         };

        //         _workContext.CurrentCustomer.BillingAddress = address;
        //         await _customerService.UpdateBillingAddress(address, _workContext.CurrentCustomer.Id);

        //         model.CurrentStep = 3;
        //         model.PaymentMethods = (await LoadAvailablePaymentMethods()).ToList();
        //         return View("~/Views/Membership/SignUp.cshtml", model);
        //     }

        //     // Step 3: Payment Method
        //     if (model.CurrentStep == 3)
        //     {
        //         if (string.IsNullOrWhiteSpace(model.SelectedPaymentMethod))
        //         {
        //             ModelState.AddModelError(nameof(model.SelectedPaymentMethod), "Please select a payment method.");
        //             model.PaymentMethods = (await LoadAvailablePaymentMethods()).ToList();
        //             return View("~/Views/Membership/SignUp.cshtml", model);
        //         }

        //         await _customerService.UpdateUserField(_workContext.CurrentCustomer,
        //             SystemCustomerFieldNames.SelectedPaymentMethod,
        //             model.SelectedPaymentMethod,
        //             _storeContext.CurrentStore.Id);

        //         return await ProcessMembershipOrder(model.SelectedPlan);
        //     }

        //     return View("~/Views/Membership/SignUp.cshtml", model);
        // }

        // private async Task<IActionResult> ProcessMembershipOrder(string selectedPlan)
        // {
        //     var plan = (await _settingService.LoadSetting<MembershipSettings>())
        //         .Plans.FirstOrDefault(p => p.SystemName == selectedPlan);

        //     if (plan == null)
        //     {
        //         ModelState.AddModelError("", "Invalid membership plan selected.");
        //         return View("~/Views/Membership/SignUp.cshtml", new MembershipModel
        //         {
        //             CurrentStep = 1,
        //             AvailablePlans = (await _settingService.LoadSetting<MembershipSettings>()).Plans.Select(p => new MembershipPlan
        //             {
        //                 Role = p.Role,
        //                 Price = p.Price,
        //                 SystemName = p.SystemName
        //             }).ToList(),
        //             PaymentMethods = (await LoadAvailablePaymentMethods()).ToList()
        //         });
        //     }

        // Store the membership price in custom fields so the command handler can access it
        //         await _customerService.UpdateUserField(_workContext.CurrentCustomer,
        //                 "MembershipOrderTotal",
        //             plan.Price,
        //             _storeContext.CurrentStore.Id);

        //         var placeOrderResult = await _mediator.Send(new PlaceOrderCommand());

        //             if (placeOrderResult.Success)
        //             {
        //                 // Activate membership - implement your membership service
        //                 // await _membershipService.ActivateMembership(...);

        //                 if (placeOrderResult.PaymentTransaction != null)
        //                 {
        //                     var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(
        //                         placeOrderResult.PaymentTransaction.PaymentMethodSystemName);

        //                     if (paymentMethod != null && paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
        //                     {
        //                         return RedirectToRoute("CheckoutPaymentInfo", new
        //                         {
        //                             paymentTransactionId = placeOrderResult.PaymentTransaction.Id
        //     });
        //                     }
        //                 }

        //                 return RedirectToRoute("CheckoutCompleted", new
        //                 {
        //                     orderId = placeOrderResult.PlacedOrder.Id
        //                 });
        //             }

        //             foreach (var error in placeOrderResult.Errors)
        // {
        //     ModelState.AddModelError("", error);
        // }

        // return View("~/Views/Membership/SignUp.cshtml", new MembershipModel
        // {
        //     CurrentStep = 3,
        //     AvailablePlans = (await _settingService.LoadSetting<MembershipSettings>()).Plans.Select(p => new MembershipPlan
        //     {
        //         Role = p.Role,
        //         Price = p.Price,
        //         SystemName = p.SystemName
        //     }).ToList(),
        //     PaymentMethods = (await LoadAvailablePaymentMethods()).ToList(),
        //     SelectedPaymentMethod = _workContext.CurrentCustomer.GetUserFieldFromEntity<string>(
        //         SystemCustomerFieldNames.SelectedPaymentMethod,
        //         _storeContext.CurrentStore.Id)
        // });
        //         }

        //         private async Task<IList<Grand.Web.Models.Checkout.CheckoutPaymentMethodModel.PaymentMethodModel>> LoadAvailablePaymentMethods()
        // {
        //     var filterByCountryId = _workContext.CurrentCustomer.BillingAddress?.CountryId ?? "";

        //     var paymentMethods = await _mediator.Send(new GetPaymentMethod
        //     {
        //         Currency = _workContext.WorkingCurrency,
        //         Customer = _workContext.CurrentCustomer,
        //         FilterByCountryId = filterByCountryId,
        //         Language = _workContext.WorkingLanguage,
        //         Store = _storeContext.CurrentStore
        //     });

        //     return paymentMethods.PaymentMethods;
        // }

        private AddressModel GetCustomerBillingAddressModel()
        {
            var billingAddress = _workContext.CurrentCustomer.BillingAddress;

            if (billingAddress == null)
                return new AddressModel(); // return empty model if no billing address

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
        [NonAction]
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
                Cart = new List<ShoppingCartItem>(), //Empty since not needed
                Customer = customer,
                Currency = currency,
                Language = language,
                Store = store,
                FilterByCountryId = filterByCountryId
            });

            return model.PaymentMethods.ToList();
        }
    }
}