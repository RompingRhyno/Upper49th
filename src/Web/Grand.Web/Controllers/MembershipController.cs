using Grand.Business.Core.Interfaces.Customers;
using Grand.Domain.Customers;
using Grand.Web.Common.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Controllers;


public class MembershipController : BasePublicController {

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }
}