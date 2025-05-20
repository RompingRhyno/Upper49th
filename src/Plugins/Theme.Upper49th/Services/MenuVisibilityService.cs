using Grand.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace Plugins.Theme.Upper49th.Services
{
    public class MenuVisibilityService
    {
        private readonly IConfiguration _configuration;

        public MenuVisibilityService(IConfiguration configuration)
        {
            _configuration = configuration;
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

            return isInstalled;
        }
    }
}
