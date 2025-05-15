using System.Collections.Generic; // Required for IList<>
using System.Threading.Tasks;
using Grand.Business.Core.Interfaces.Cms;

namespace Customer.Membership;

public class MyPluginWidgetProvider : IWidgetProvider
{
    public string ConfigurationUrl => "/Admin/Membership/Configure";

    public string SystemName => "Customer.Membership";

    public string FriendlyName => "Misc MyPlugin";

    public int Priority => 0;

    public IList<string> LimitedToStores => new List<string>();

    public IList<string> LimitedToGroups => new List<string>();

    public Task<string> GetPublicViewComponentName(string widgetZone)
    {
        return Task.FromResult("MyPluginWidget");
    }

    public Task<IList<string>> GetWidgetZones()
    {
        return Task.FromResult<IList<string>>(new List<string> { "home_page_top" });
    }
}