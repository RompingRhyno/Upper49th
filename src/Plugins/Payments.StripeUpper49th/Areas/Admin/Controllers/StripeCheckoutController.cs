﻿using Grand.Business.Core.Interfaces.Common.Configuration;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Security;
using Grand.Domain.Permissions;
using Grand.Web.Common.Controllers;
using Grand.Web.Common.Filters;
using Grand.Web.Common.Helpers;
using Grand.Web.Common.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.StripeUpper49th.Models;

namespace Payments.StripeUpper49th.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area("Admin")]
[PermissionAuthorize(PermissionSystemName.PaymentMethods)]
public class StripeCheckoutController : BasePaymentController
{
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly ITranslationService _translationService;
    private readonly IAdminStoreService _adminStoreService;

    public StripeCheckoutController(
        ISettingService settingService,
        ITranslationService translationService,
        IPermissionService permissionService,
        IAdminStoreService adminStoreService)
    {
        _adminStoreService = adminStoreService;
        _settingService = settingService;
        _translationService = translationService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.Authorize(StandardPermission.ManagePaymentMethods))
            return AccessDeniedView();

        //load settings for a chosen store scope
        var storeScope = await _adminStoreService.GetActiveStore();
        var stripeCheckoutPaymentSettings = await _settingService.LoadSetting<StripeCheckoutPaymentSettings>(storeScope);

        var model = new ConfigurationModel {
            ApiKey = stripeCheckoutPaymentSettings.ApiKey,
            WebhookEndpointSecret = stripeCheckoutPaymentSettings.WebhookEndpointSecret,
            Description = stripeCheckoutPaymentSettings.Description,
            Line = stripeCheckoutPaymentSettings.Line,
            DisplayOrder = stripeCheckoutPaymentSettings.DisplayOrder,
            StoreScope = storeScope
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.Authorize(StandardPermission.ManagePaymentMethods))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Configure();

        //load settings for a chosen store scope
        var storeScope = await _adminStoreService.GetActiveStore();
        var stripeCheckoutPaymentSettings = await _settingService.LoadSetting<StripeCheckoutPaymentSettings>(storeScope);

        //save settings
        stripeCheckoutPaymentSettings.ApiKey = model.ApiKey;
        stripeCheckoutPaymentSettings.WebhookEndpointSecret = model.WebhookEndpointSecret;
        stripeCheckoutPaymentSettings.Description = model.Description;
        stripeCheckoutPaymentSettings.Line = model.Line;
        stripeCheckoutPaymentSettings.DisplayOrder = model.DisplayOrder;

        await _settingService.SaveSetting(stripeCheckoutPaymentSettings, storeScope);

        //now clear settings cache
        await _settingService.ClearCache();

        Success(_translationService.GetResource("Admin.Plugins.Saved"));

        return await Configure();
    }
}