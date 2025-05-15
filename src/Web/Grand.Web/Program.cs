using Grand.Web.Common.Extensions;
using Grand.Web.Common.Startup;
using Grand.Web.Filters;
using StartupBase = Grand.Infrastructure.StartupBase;

var builder = WebApplication.CreateBuilder(args);

//add configuration
builder.Configuration.AddAppSettingsJsonFile(args);

//add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:5050", "https://upper49th.fly.dev")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

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

//use CORS policy
app.UseCors("AllowSpecificOrigins");

//request pipeline
StartupBase.ConfigureRequestPipeline(app, builder.Environment);

//run app
await app.RunAsync();