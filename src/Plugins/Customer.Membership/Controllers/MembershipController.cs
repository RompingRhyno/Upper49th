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


        public MembershipController(
            ILogger<MembershipController> logger,
            IContextAccessor contextAccessor,
            IGroupService groupService,
            ICustomerService customerService,
            ISettingService settingService)
        {
            _logger = logger;
            _workContext = contextAccessor.WorkContext;
            _storeContext = contextAccessor.StoreContext;
            _groupService = groupService;
            _customerService = customerService;
            _settingService = settingService;
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

        [HttpPost]
        public async Task<IActionResult> Pay()
        {
            return Redirect("/");
        }

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
    }
}