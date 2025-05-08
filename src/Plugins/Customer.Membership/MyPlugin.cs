using System;
using System.Threading.Tasks;
using Grand.Infrastructure;
using Grand.Infrastructure.Plugins;
using Grand.Business.Core.Interfaces.Cms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Customer.Membership;

public class MyPlugin : BasePlugin, IPlugin
{
    public override string ConfigurationUrl()
    {
        return "/Admin/MyPlugin/Configure";
    }

    public override async Task Install()
    {
        await base.Install();
    }

    public override async Task Uninstall()
    {
        await base.Uninstall();
    }
}
