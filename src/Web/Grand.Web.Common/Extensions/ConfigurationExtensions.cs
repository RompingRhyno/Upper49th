﻿using Grand.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Web.Common.Extensions;

public static class ConfigurationExtensions
{
    public static void AddAppSettingsJsonFile(this IConfigurationManager configuration, string[] args)
    {
        configuration.AddJsonFile("App_Data/appsettings.json");
        configuration.AddEnvironmentVariables();
        if (args.Any())
        {
            configuration.AddCommandLine(args);
            var appSettings = configuration["appsettings"];
            if (!string.IsNullOrEmpty(appSettings))
                configuration.AddJsonFile($"App_Data/{appSettings}/appsettings.json");
        }

        //Customed by Joe: checks if environment is Development if appsettings.Development.json exists, and if it does then load as Development
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (environment == "Development")
        {
            var developmentConfigPath = "App_Data/appsettings.Development.json";
            if (File.Exists(developmentConfigPath))
            {
                configuration.AddJsonFile("App_Data/appsettings.Development.json", optional: true, reloadOnChange: true);
            }
        }
    }

    public static void ConfigureApplicationSettings(this WebApplicationBuilder builder)
    {
        //Allow non ASCII chars in headers
        var config = new AppConfig();
        builder.Configuration.GetSection("Application").Bind(config);
        if (config.AllowNonAsciiCharInHeaders)
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ResponseHeaderEncodingSelector = _ => Encoding.UTF8;
            });
        if (config.MaxRequestBodySize.HasValue)
        {
            builder.WebHost.ConfigureKestrel(host =>
            {
                host.Limits.MaxRequestBodySize = config.MaxRequestBodySize.Value;
            });

            builder.Services.Configure<FormOptions>(opt =>
            {
                opt.MultipartBodyLengthLimit = config.MaxRequestBodySize.Value;
            });
        }
    }
}