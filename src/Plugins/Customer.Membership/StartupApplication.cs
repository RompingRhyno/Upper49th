using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Business.Core.Interfaces.Cms;
using Grand.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Customer.Membership;

public class StartupApplication : IStartupApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ContextAccessor>();
        services.AddScoped<GroupService>();
        services.AddScoped<CustomerService>();
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