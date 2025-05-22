using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.StripeUpper49th.Services;

namespace Payments.StripeUpper49th;

public class StartupApplication : IStartupApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPaymentProvider, StripeCheckoutPaymentProvider>();
        services.AddScoped<IStripeCheckoutService, StripeCheckoutService>();
    }

    public int Priority => 10;

    public void Configure(WebApplication application, IWebHostEnvironment webHostEnvironment)
    {
    }

    public bool BeforeConfigure => false;
}