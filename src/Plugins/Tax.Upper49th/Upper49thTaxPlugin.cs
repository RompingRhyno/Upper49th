using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Infrastructure.Plugins;

namespace Tax.Upper49th;

/// <summary>
///     Fixed rate tax provider
/// </summary>
public class Upper49thTaxPlugin(
    IPluginTranslateResource pluginTranslateResource)
    : BasePlugin, IPlugin
{

    /// <summary>
    ///     Gets a configuration page URL
    /// </summary>
    public override string ConfigurationUrl()
    {
        return Upper49thTaxDefaults.ConfigurationUrl;
    }


    /// <summary>
    ///     Install plugin
    /// </summary>
    public override async Task Install()
    {
        //locales
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Tax.Upper49th.FriendlyName", "Upper49th Tax Provider");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.Store", "Store");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.Store.Hint",
            "If an asterisk is selected, then this shipping rate will apply to all stores.");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.Country", "Country");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.Country.Hint", "The country.");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.StateProvince", "State / province");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.StateProvince.Hint",
            "If an asterisk is selected, then this tax rate will apply to all customers from the given country, regardless of the state.");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.Zip", "Zip");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.Zip.Hint",
            "Zip / postal code. If zip is empty, then this tax rate will apply to all customers from the given country or state, regardless of the zip code.");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.TaxCategory", "Tax category");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.TaxCategory.Hint", "The tax category.");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.Percentage", "Percentage");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.Fields.Percentage.Hint", "The tax rate.");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.AddRecord", "Add tax rate");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.Tax.Upper49th.AddRecord.Hint", "Adding a new tax rate");

        await base.Install();
    }

    /// <summary>
    ///     Uninstall plugin
    /// </summary>
    public override async Task Uninstall()
    {
        //locales
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.Store");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.Store.Hint");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.Country");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.Country.Hint");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.StateProvince");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.StateProvince.Hint");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.Zip");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.Zip.Hint");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.TaxCategory");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.TaxCategory.Hint");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.Percentage");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.Fields.Percentage.Hint");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.AddRecord");
        await pluginTranslateResource.DeletePluginTranslationResource("Plugins.Tax.Upper49th.AddRecord.Hint");

        await base.Uninstall();
    }
}