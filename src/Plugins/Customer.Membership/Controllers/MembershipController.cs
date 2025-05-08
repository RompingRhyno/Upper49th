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

namespace Customer.Membership.Controllers
{
    [Route("[controller]")]
    public class CustomerMembershipController : BaseController
    {
        private readonly ILogger<CustomerMembershipController> _logger;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGroupService _groupService;


        public CustomerMembershipController(
            ILogger<CustomerMembershipController> logger,
            ContextAccessor contextAccessor,
            GroupService groupService
            )
        {
            _logger = logger;
            _workContext = contextAccessor.WorkContext;
            _storeContext = contextAccessor.StoreContext;
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var groupIds = _workContext.CurrentCustomer.Groups.ToList().ToArray();
            var groups = await _groupService.GetAllByIds(groupIds);
            var model = new { Groups = groups };
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