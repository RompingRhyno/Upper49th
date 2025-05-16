using Grand.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Plugins.Theme.Upper49th.Services
{
    public class MenuVisibilityService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MenuVisibilityService> _logger;

        public MenuVisibilityService(IConfiguration configuration, ILogger<MenuVisibilityService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public bool ShouldShowMembershipMenu()
        {
            var config = new ExtensionsConfig();
            _configuration.GetSection("Extensions").Bind(config);

            var installedPlugins = !string.IsNullOrWhiteSpace(config.InstalledPlugins)
                ? config.InstalledPlugins
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                : Enumerable.Empty<string>();

            bool isInstalled = installedPlugins
                .Contains("Customer.Membership", StringComparer.OrdinalIgnoreCase);

            // Log to the system logger
            _logger.LogInformation("InstalledPlugins: {Installed}", config.InstalledPlugins);
            _logger.LogInformation("Customer.Membership plugin installed? {IsInstalled}", isInstalled);

            return isInstalled;
        }
    }
}
