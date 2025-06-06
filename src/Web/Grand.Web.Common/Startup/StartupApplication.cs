using Grand.Business.Core.Interfaces.Common.Pdf;
using Grand.Business.Core.Interfaces.System.Admin;
using Grand.Data;
using Grand.Infrastructure;
using Grand.Infrastructure.Caching;
using Grand.Infrastructure.Caching.Message;
using Grand.Infrastructure.Caching.Redis;
using Grand.Infrastructure.Configuration;
using Grand.Infrastructure.Validators;
using Grand.Web.Common.Helpers;
using Grand.Web.Common.Localization;
using Grand.Web.Common.Menu;
using Grand.Web.Common.Middleware;
using Grand.Web.Common.Page;
using Grand.Web.Common.Routing;
using Grand.Web.Common.Security.Captcha;
using Grand.Web.Common.TagHelpers;
using Grand.Web.Common.Themes;
using Grand.Web.Common.Validators;
using Grand.Web.Common.View;
using Grand.Web.Common.ViewRender;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;

namespace Grand.Web.Common.Startup;

/// <summary>
///     Startup application
/// </summary>
public class StartupApplication : IStartupApplication
{
    /// <summary>
    ///     Register services and interfaces
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configuration">Config</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        RegisterCache(services, configuration);

        RegisterContextService(services);

        RegisterFramework(services);
    }

    public void Configure(WebApplication application, IWebHostEnvironment webHostEnvironment)
    {
    }

    public int Priority => 0;
    public bool BeforeConfigure => false;

    private static void RegisterCache(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var config = new RedisConfig();
        configuration.GetSection("Redis").Bind(config);

        serviceCollection.AddSingleton<ICacheBase, MemoryCacheBase>();

        if (config.RedisPubSubEnabled)
        {
            var redis = ConnectionMultiplexer.Connect(config.RedisPubSubConnectionString);
            serviceCollection.AddSingleton(_ => redis.GetSubscriber());
            serviceCollection.AddSingleton<IMessageBus, RedisMessageBus>();
            serviceCollection.AddSingleton<ICacheBase, RedisMessageCacheManager>();
            return;
        }
    }

    private static void RegisterContextService(IServiceCollection serviceCollection)
    {
        //work context
        serviceCollection.AddSingleton<IContextAccessor, ContextAccessor>();
        serviceCollection.AddScoped<IWorkContextSetter, WorkContextSetter>();
        serviceCollection.AddScoped<IStoreContextSetter, StoreContextSetter>();
        serviceCollection.AddScoped<IAdminStoreService, AdminStoreService>();
        //View factory
        serviceCollection.AddScoped<IViewFactory, ViewFactory>();

        //Default view area
        serviceCollection.AddScoped<IAreaViewFactory, DefaultAreaViewFactory>();

        //Theme context factory
        serviceCollection.AddScoped<IThemeContextFactory, ThemeContextFactory>();

        //Default theme context
        serviceCollection.AddScoped<IThemeContext, ThemeContext>();

        //Default theme view
        serviceCollection.AddScoped<IThemeView, DefaultThemeView>();
        
        //Admin site map service
        serviceCollection.AddScoped<IAdminSiteMapService, AdminSiteMapService>();        
    }


    private static void RegisterFramework(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPageHeadBuilder, PageHeadBuilder>();

        serviceCollection.AddScoped<SlugRouteTransformer>();

        serviceCollection.AddScoped<IResourceManager, ResourceManager>();

        serviceCollection.AddScoped<IValidatorFactory, ValidatorFactory>();

        serviceCollection.AddScoped<IEnumTranslationService, EnumTranslationService>();
        if (DataSettingsManager.DatabaseIsInstalled())
        {
            serviceCollection.AddScoped<LocService>();
        }
        else
        {
            var provider = serviceCollection.BuildServiceProvider();
            var tmp = provider.GetRequiredService<IStringLocalizerFactory>();
            serviceCollection.AddScoped(_ => new LocService(tmp));
        }

        //powered by
        serviceCollection.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();

        //request reCAPTCHA service
        serviceCollection.AddHttpClient<GoogleReCaptchaValidator>();

        serviceCollection.AddScoped<IViewRenderService, ViewRenderService>();
    }
}