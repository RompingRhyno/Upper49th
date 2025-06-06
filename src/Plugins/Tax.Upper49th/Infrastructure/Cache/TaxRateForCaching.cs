namespace Tax.Upper49th.Infrastructure.Cache;

/// <summary>
///     Represents a tax rate
/// </summary>
public class TaxRateForCaching
{
    public string Id { get; set; }
    public string StoreId { get; set; }
    public string TaxCategoryId { get; set; }
    public string CountryId { get; set; }
    public string StateProvinceId { get; set; }
    public string Zip { get; set; }
    public double Percentage { get; set; }
}