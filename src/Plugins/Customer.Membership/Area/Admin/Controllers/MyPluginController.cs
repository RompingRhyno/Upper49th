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
using Customer.Membership.Models;

[AuthorizeAdmin]
[Area("Admin")]
public class MembershipController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly MembershipSettings _MembershipSettings;

    public MembershipController(ISettingService settingService, MembershipSettings MembershipSettings)
    {
        _settingService = settingService;
        _MembershipSettings = MembershipSettings;
    }

    [HttpGet]
    public IActionResult Configure()
    {
        var model = new SettingsModel()
        {
            ApiKey = _MembershipSettings.ApiKey,
            UseSandbox = _MembershipSettings.UseSandbox
        };
        Console.WriteLine("ABOUT TO DO SOMETHING!!!");
        Console.WriteLine(model.ApiKey);
        return View("~/Area/Admin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(SettingsModel model)
    {
        _MembershipSettings.UseSandbox = model.UseSandbox;
        _MembershipSettings.ApiKey = model.ApiKey;
        await _settingService.SaveSetting(_MembershipSettings);
        Success("Success");
        return await Task.FromResult(Configure());
    }

}
