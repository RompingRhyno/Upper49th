using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Customer.Membership.Controllers
{
    [Route("[controller]")]
    public class MembershipController : Controller
    {
        private readonly ILogger<MembershipController> _logger;

        public MembershipController(ILogger<MembershipController> logger)
        {
            _logger = logger;
        }

        [HttpGet("membership", Name = "MembershipIndex")]
        public IActionResult Index()
        {
            return View("~/Views/Membership/Index.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}