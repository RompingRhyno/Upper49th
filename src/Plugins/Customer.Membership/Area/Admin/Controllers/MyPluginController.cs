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
public class MyPluginController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly MyPluginSettings _myPluginSettings;

    public MyPluginController(ISettingService settingService, MyPluginSettings myPluginSettings)
    {
        _settingService = settingService;
        _myPluginSettings = myPluginSettings;
    }

    [HttpGet]
    public IActionResult Configure()
    {
        var model = new SettingsModel()
        {
            ApiKey = _myPluginSettings.ApiKey,
            UseSandbox = _myPluginSettings.UseSandbox
        };
        Console.WriteLine("ABOUT TO DO SOMETHING!!!");
        Console.WriteLine(model.ApiKey);
        return View("~/Area/Admin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(SettingsModel model)
    {
        _myPluginSettings.UseSandbox = model.UseSandbox;
        _myPluginSettings.ApiKey = model.ApiKey;
        await _settingService.SaveSetting(_myPluginSettings);
        Success("Success");
        return await Task.FromResult(Configure());
    }

}
