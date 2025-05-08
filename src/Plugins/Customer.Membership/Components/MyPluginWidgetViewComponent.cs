using Grand.Infrastructure;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Customer.Membership.Components;

[ViewComponent(Name = "MyPluginWidget")]
public class MyPluginWidgetViewComponent : ViewComponent
{
    public async Task<IViewComponentResult>  InvokeAsync(string widgetZone, object additionalData = null)
    {
        var model = new { Value = "Sample value from MyPluginWidget" };
        return View("~/Views/MyPluginWidget/Index.cshtml", model);
    }
}