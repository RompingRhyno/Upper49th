using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Business.Core.Interfaces.Cms;
using Grand.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grand.Web.Common;
using Grand.Business.Common.Services.Directory;
using Grand.Business.Customers.Services;

namespace Customer.Membership;

public class StartupApplication : IStartupApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IContextAccessor, ContextAccessor>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<ICustomerService, CustomerService>();
    }

    public int Priority => 10;

    public void Configure(WebApplication application, IWebHostEnvironment webHostEnvironment)
    {
    }

    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        {
            endpoints.MapControllerRoute(
            name: "MembershipIndex",
            pattern: "membership",
            defaults: new { controller = "Membership", action = "Index" }
            );
        }
    }

    public bool BeforeConfigure => false;
}