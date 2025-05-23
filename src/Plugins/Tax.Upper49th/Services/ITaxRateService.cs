using Grand.Domain;
using Tax.Upper49th.Domain;

namespace Tax.Upper49th.Services;

/// <summary>
///     Tax rate service interface
/// </summary>
public interface ITaxRateService
{
    /// <summary>
    ///     Deletes a tax rate
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    Task DeleteTaxRate(TaxRate taxRate);

    /// <summary>
    ///     Gets all tax rates
    /// </summary>
    /// <returns>Tax rates</returns>
    Task<IPagedList<TaxRate>> GetAllTaxRates(int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    ///     Gets a tax rate
    /// </summary>
    /// <param name="taxRateId">Tax rate identifier</param>
    /// <returns>Tax rate</returns>
    Task<TaxRate> GetTaxRateById(string taxRateId);

    /// <summary>
    ///     Inserts a tax rate
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    Task InsertTaxRate(TaxRate taxRate);

    /// <summary>
    ///     Updates the tax rate
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    Task UpdateTaxRate(TaxRate taxRate);
}