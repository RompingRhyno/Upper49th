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
        var allRoleNames = allRoles.Select(g => g.Name).ToList();

        var settingPlans = settings.Plans ?? new List<MembershipPlan>();
        var removedPlans = settingPlans
        .Where(p => !allRoleNames.Contains(p.Role))
        .ToList();
        if (removedPlans.Any())
        {
            foreach (var removed in removedPlans)
            {
                Console.WriteLine($"Removing non-existent role: {removed.Role}");
                settingPlans.Remove(removed);
            }

            // Save the cleaned settings
            settings.Plans = settingPlans;
            await _settingService.SaveSetting(settings);
        }


        var configuredRoles = settingPlans.Select(p => p.Role);
        var availableRoles = allRoles
        .Where(g => !configuredRoles.Contains(g.Name))
        .Select(g => g.Name)
        .Where(g => g.Contains("Member"))
        .ToList();

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
    public async Task<IActionResult> Configure(SettingsModel model, string newRole, string newPrice, string newDescription, List<string> RolesToDelete)
    {

        var settings = await _settingService.LoadSetting<MembershipSettings>();

        var updatedPlans = model.Plans ?? new List<MembershipPlan>();

        if (RolesToDelete != null && RolesToDelete.Any())
        {
            updatedPlans = updatedPlans.Where(plan => !RolesToDelete.Contains(plan.Role)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(newRole) && decimal.TryParse(newPrice, out var price) && !updatedPlans.Any(p => p.Role.Equals(newRole)))
        {

            var allRoles = await _groupService.GetAllCustomerGroups(showHidden: true);
            var matchingGroup = allRoles.FirstOrDefault(g => g.Name == newRole);

            if (matchingGroup == null)
            {
                ModelState.AddModelError("", "The selected role was not found.");
                return View("~/Area/Admin/Views/Configure.cshtml", model);
            }
            updatedPlans.Add(new MembershipPlan
            {
                Role = newRole,
                SystemName = matchingGroup.SystemName,
                Price = price,
                Description = newDescription
            });
            Console.WriteLine("New role SystemName" + matchingGroup.SystemName);
        }

        settings.Plans = updatedPlans;

        await _settingService.SaveSetting(settings);
        Success("Success");

        return RedirectToAction(nameof(Configure));
    }

}
