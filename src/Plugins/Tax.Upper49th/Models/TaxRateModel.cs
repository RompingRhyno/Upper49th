using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;

namespace Tax.Upper49th.Models;

public class TaxRateModel : BaseEntityModel
{
    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Store")]
    public string StoreId { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Store")]
    public string StoreName { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.TaxCategory")]
    public string TaxCategoryId { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.TaxCategory")]
    public string TaxCategoryName { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Country")]
    public string CountryId { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Country")]
    public string CountryName { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.StateProvince")]
    public string StateProvinceId { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.StateProvince")]
    public string StateProvinceName { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Zip")]
    public string Zip { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Percentage")]
    public double Percentage { get; set; }
}