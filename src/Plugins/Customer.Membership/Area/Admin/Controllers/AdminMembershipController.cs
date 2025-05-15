using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Grand.Domain.Configuration;
using Grand.Web.Common.Controllers;
using Grand.Web.Common.Filters;
using Grand.Business.Core.Interfaces.Common.Configuration;
using Customer.Membership.Domain.Settings;
using Customer.Membership.Models;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Business.Core.Interfaces.Common.Directory;

[AuthorizeAdmin]
[Area("Admin")]
public class AdminMembershipController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly IGroupService _groupService;

    public AdminMembershipController(ISettingService settingService, IGroupService groupService)
    {
        _settingService = settingService;
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<IActionResult> Configure()
    {
        var settings = await _settingService.LoadSetting<MembershipSettings>();

        var allRoles = await _groupService.GetAllCustomerGroups(showHidden: true);
        var configuredRoles = settings.Plans?.Select(p => p.Role) ?? new List<string>();

        Console.WriteLine("Configured roles " + (configuredRoles.FirstOrDefault()));

        var availableRoles = allRoles
        .Where(g => !configuredRoles.Contains(g.Name))
        .Select(g => g.Name)
        .Where(g => g.Contains("Member"))
        .ToList();

        var settingPlans = settings.Plans;
        if (settingPlans.Any())
        {
            Console.WriteLine("SettingPlans roles " + (settingPlans.FirstOrDefault().Role));
        }

        var model = new SettingsModel()
        {
            Plans = settingPlans,
            AvailableRoles = availableRoles
        };
        return View("~/Area/Admin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(SettingsModel model, string newRole, string newPrice, List<string> RolesToDelete)
    {

        var settings = await _settingService.LoadSetting<MembershipSettings>();

        var updatedPlans = settings.Plans?.ToList() ?? new List<MembershipPlan>();

        if (RolesToDelete != null && RolesToDelete.Any())
        {
            updatedPlans = updatedPlans.Where(plan => !RolesToDelete.Contains(plan.Role)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(newRole) && decimal.TryParse(newPrice, out var price) && !updatedPlans.Any(p => p.Role.Equals(newRole)))
        {
            updatedPlans.Add(new MembershipPlan
            {
                Role = newRole,
                Price = price
            });
        }

        Console.WriteLine("New role " + newRole);

        settings.Plans = updatedPlans;

        await _settingService.SaveSetting(settings);
        Success("Success");

        return RedirectToAction(nameof(Configure));
    }

}
