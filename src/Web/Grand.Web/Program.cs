using Grand.Web.Common.Extensions;
using Grand.Web.Common.Startup;
using StartupBase = Grand.Infrastructure.StartupBase;

var builder = WebApplication.CreateBuilder(args);

//add configuration
builder.Configuration.AddAppSettingsJsonFile(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalhostPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5050")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

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

//build app
var app = builder.Build();

//request pipeline
StartupBase.ConfigureRequestPipeline(app, builder.Environment);

app.UseCors("LocalhostPolicy");

//run app
await app.RunAsync();