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

namespace Customer.Membership.Controllers
{
    [Route("[controller]")]
    public class MembershipController : Controller
    {
        private readonly ILogger<MembershipController> _logger;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGroupService _groupService;
        private readonly ICustomerService _customerService;


        public MembershipController(
            ILogger<MembershipController> logger,
            ContextAccessor contextAccessor,
            GroupService groupService,
            CustomerService customerService)
        {
            _logger = logger;
            _workContext = contextAccessor.WorkContext;
            _storeContext = contextAccessor.StoreContext;
            _groupService = groupService;
            _customerService = customerService;
        }

        [HttpGet("membership", Name = "MembershipIndex")]
        public IActionResult Index()
        {
            return View("~/Views/Membership/Index.cshtml");
        }

        [HttpGet]
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