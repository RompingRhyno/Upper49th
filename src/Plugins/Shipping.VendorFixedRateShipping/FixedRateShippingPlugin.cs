using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Infrastructure.Plugins;

namespace Shipping.VendorFixedRateShipping;

/// <summary>
///     Fixed rate shipping computation method
/// </summary>
public class FixedRateShippingPlugin(
    IPluginTranslateResource pluginTranslateResource)
    : BasePlugin, IPlugin
{
    #region Methods

    /// <summary>
    ///     Install plugin
    /// </summary>
    public override async Task Install()
    {
        //locales
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Shipping.VendorFixedRate.FriendlyName", "Vendor shipping fixed rate");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Shipping.VendorFixedRateShipping.Fields.ShippingMethodName", "Shipping method");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Shipping.VendorFixedRateShipping.Fields.Rate", "Rate");

        await base.Install();
    }


    /// <summary>
    ///     Uninstall plugin
    /// </summary>
    public override async Task Uninstall()
    {
        //locales
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Shipping.VendorFixedRateShipping.Fields.ShippingMethodName");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Shipping.VendorFixedRateShipping.Fields.Rate");
        await pluginTranslateResource.DeletePluginTranslationResource("Shipping.VendorFixedRate.FriendlyName");

        await base.Uninstall();
    }

    #endregion
}