using Grand.Web.Common.Extensions;
using Grand.Web.Common.Startup;
using Grand.Web.Filters;
using StartupBase = Grand.Infrastructure.StartupBase;

var builder = WebApplication.CreateBuilder(args);

//add configuration
builder.Configuration.AddAppSettingsJsonFile(args);

builder.AddServiceDefaults();

builder.Host.UseDefaultServiceProvider((_, options) =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = false;
});

//add services
StartupBase.ConfigureServices(builder.Services, builder.Configuration);

builder.ConfigureApplicationSettings();

//register task
builder.Services.RegisterTasks(builder.Configuration);

//custom filter for checking if users are registered
builder.Services.AddScoped<RequireRegisteredCustomerFilter>();

//build app
var app = builder.Build();

app.Use(async (context, next) =>
{
    var host = context.Request.Host.Host;

    if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
    {
        // Remove "www." prefix to redirect to root domain
        var nonWwwHost = host.Substring(4);

        var newUrl = $"{context.Request.Scheme}://{nonWwwHost}{context.Request.Path}{context.Request.QueryString}";
        context.Response.StatusCode = 301; // Permanent redirect
        context.Response.Headers.Location = newUrl;
        return;
    }

    await next();
});


//request pipeline
StartupBase.ConfigureRequestPipeline(app, builder.Environment);

//run app
await app.RunAsync();