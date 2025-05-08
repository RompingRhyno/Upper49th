using Grand.Web.Common.Themes;

namespace Theme.Upper49th;

public class Upper49thThemeView : IThemeView
{
    public string AreaName => "";
    public string ThemeName => "Upper49th";

    public ThemeInfo ThemeInfo => new("Upper49th", "~/Plugins/Theme.Upper49th/Content/theme.jpg",
        "Upper49th", false);

    public IEnumerable<string> GetViewLocations()
    {
        return new List<string> {
            "/Views/Upper49th/{1}/{0}.cshtml",
            "/Views/Upper49th/Shared/{0}.cshtml",
            "/Views/{1}/{0}.cshtml",
            "/Views/Shared/{0}.cshtml"
        };
    }
}