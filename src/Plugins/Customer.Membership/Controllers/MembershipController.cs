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



        public MembershipController(
            ILogger<MembershipController> logger,
            IContextAccessor contextAccessor,
            IGroupService groupService,
            ICustomerService customerService,
            ISettingService settingService,
            IMediator mediator,
            AddressSettings addressSettings)
        {
            _logger = logger;
            _workContext = contextAccessor.WorkContext;
            _storeContext = contextAccessor.StoreContext;
            _groupService = groupService;
            _customerService = customerService;
            _settingService = settingService;
            _mediator = mediator;
            _addressSettings = addressSettings;
        }

        [HttpGet("", Name = "MembershipIndex")]
        public IActionResult Index()
        {
            return View("~/Views/Membership/Index.cshtml");
        }

        [HttpGet("signup")]
        public async Task<IActionResult> SignUp()
        {

            var settings = await _settingService.LoadSetting<MembershipSettings>();

            var model = new MembershipModel
            {
                AvailablePlans = settings.Plans.Select(p => new MembershipPlan
                {
                    Role = p.Role,
                    Price = p.Price
                }).ToList()
            };

            return View("~/Views/Membership/SignUp.cshtml", model);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(MembershipModel model)
        {
            var settings = await _settingService.LoadSetting<MembershipSettings>();

            // Load available plans from settings (configured in admin)
            model.AvailablePlans = settings.Plans.Select(p => new MembershipPlan
            {
                Role = p.Role,
                Price = p.Price
            }).ToList();

            // Step 1 - select the plan
            if (model.CurrentStep == 1)
            {
                if (string.IsNullOrWhiteSpace(model.SelectedPlan))
                {
                    ModelState.AddModelError(nameof(model.SelectedPlan), "Please select a plan.");
                    return View("~/Views/Membership/SignUp.cshtml", model);
                }

                // Initial Step 2
                model.CurrentStep = 2;
                model.PaymentMethods = await LoadAvailablePaymentMethods();

                return View("~/Views/Membership/SignUp.cshtml", model);
            }

            // TO-DO inbetween billing address (partial currently not used)

            // Step 2 - payment method selection
            else if (model.CurrentStep == 2)
            {
                model.PaymentMethods = await LoadAvailablePaymentMethods();

                if (string.IsNullOrWhiteSpace(model.SelectedPaymentMethod))
                {
                    ModelState.AddModelError(nameof(model.SelectedPaymentMethod), "Please select a payment method.");
                    return View("~/Views/Membership/SignUp.cshtml", model);
                }

                return RedirectToAction("/");
            }

            return View("~/Views/Membership/SignUp.cshtml", model);
        }

        // Currently unused, will be used for later to assign roles once payment goes through
        [HttpGet("success")]
        public async Task<IActionResult> Success()
        {
            var groupIds = _workContext.CurrentCustomer.Groups.ToList().ToArray();
            var groups = await _groupService.GetAllByIds(groupIds);
            var model = new { Groups = groups };
            var memberGroup = _groupService.GetCustomerGroupBySystemName("Member");
            var memberGroupResult = await memberGroup;
            _customerService.InsertCustomerGroupInCustomer(memberGroupResult, _workContext.CurrentCustomer.Id);
            foreach (var group in groups)
            {
                if (group.Name == "Registered")
                {
                    return View("~/Views/Groups.cshtml", model);
                }
            }
            return Redirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        // Helper function to get payment methods
        [NonAction]
        private async Task<List<CheckoutPaymentMethodModel.PaymentMethodModel>> LoadAvailablePaymentMethods()
        {
            var customer = _workContext.CurrentCustomer;
            var store = _storeContext.CurrentStore;
            var language = _workContext.WorkingLanguage;
            var currency = _workContext.WorkingCurrency;

            //For country filtering - need to add billing address first
            // var filterByCountryId = "";
            // if (_addressSettings.CountryEnabled &&
            //     customer.BillingAddress != null &&
            //     !string.IsNullOrEmpty(customer.BillingAddress.CountryId))
            // {
            //     filterByCountryId = customer.BillingAddress.CountryId;
            // }

            var model = await _mediator.Send(new GetPaymentMethod
            {
                Cart = new List<ShoppingCartItem>(), //Empty since not needed
                Customer = customer,
                Currency = currency,
                Language = language,
                Store = store,
                FilterByCountryId = null //filterByCountryId
            });

            return model.PaymentMethods.ToList();
        }
    }
}