using Grand.Business.Core.Interfaces.Catalog.Tax;
using Grand.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tax.Upper49th.Services;

namespace Tax.Upper49th;

public class StartupApplication : IStartupApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITaxRateService, TaxRateService>();
        services.AddScoped<ITaxProvider, Upper49thTaxProvider>();
    }

    public int Priority => 20;

    public void Configure(WebApplication application, IWebHostEnvironment webHostEnvironment)
    {
    }

    public bool BeforeConfigure => false;
}