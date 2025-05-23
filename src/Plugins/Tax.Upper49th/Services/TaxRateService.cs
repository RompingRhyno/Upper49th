using Grand.Data;
using Grand.Domain;
using Grand.Infrastructure.Caching;
using Grand.Infrastructure.Extensions;
using MediatR;
using Tax.Upper49th.Domain;

namespace Tax.Upper49th.Services;

/// <summary>
///     Tax rate service
/// </summary>
public class TaxRateService : ITaxRateService
{
    #region Ctor

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="mediator">Mediator</param>
    /// <param name="cacheManager">Cache manager</param>
    /// <param name="taxRateRepository">Tax rate repository</param>
    public TaxRateService(IMediator mediator,
        ICacheBase cacheManager,
        IRepository<TaxRate> taxRateRepository)
    {
        _mediator = mediator;
        _cacheBase = cacheManager;
        _taxRateRepository = taxRateRepository;
    }

    #endregion

    #region Constants

    private const string TAXRATE_ALL_KEY = "Grand.taxrate.all-{0}-{1}";
    private const string TAXRATE_PATTERN_KEY = "Grand.taxrate.";

    #endregion

    #region Fields

    private readonly IMediator _mediator;
    private readonly IRepository<TaxRate> _taxRateRepository;
    private readonly ICacheBase _cacheBase;

    #endregion

    #region Methods

    /// <summary>
    ///     Deletes a tax rate
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    public virtual async Task DeleteTaxRate(TaxRate taxRate)
    {
        ArgumentNullException.ThrowIfNull(taxRate);

        await _taxRateRepository.DeleteAsync(taxRate);

        await _cacheBase.RemoveByPrefix(TAXRATE_PATTERN_KEY);

        //event notification
        await _mediator.EntityDeleted(taxRate);
    }

    /// <summary>
    ///     Gets all tax rates
    /// </summary>
    /// <returns>Tax rates</returns>
    public virtual async Task<IPagedList<TaxRate>> GetAllTaxRates(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var key = string.Format(TAXRATE_ALL_KEY, pageIndex, pageSize);
        return await _cacheBase.GetAsync(key, async () =>
        {
            var query = from tr in _taxRateRepository.Table
                orderby tr.StoreId, tr.CountryId, tr.StateProvinceId, tr.Zip, tr.TaxCategoryId
                select tr;
            return await Task.FromResult(new PagedList<TaxRate>(query, pageIndex, pageSize));
        });
    }

    /// <summary>
    ///     Gets a tax rate
    /// </summary>
    /// <param name="taxRateId">Tax rate identifier</param>
    /// <returns>Tax rate</returns>
    public virtual Task<TaxRate> GetTaxRateById(string taxRateId)
    {
        return _taxRateRepository.GetByIdAsync(taxRateId);
    }

    /// <summary>
    ///     Inserts a tax rate
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    public virtual async Task InsertTaxRate(TaxRate taxRate)
    {
        ArgumentNullException.ThrowIfNull(taxRate);

        await _taxRateRepository.InsertAsync(taxRate);

        await _cacheBase.RemoveByPrefix(TAXRATE_PATTERN_KEY);

        //event notification
        await _mediator.EntityInserted(taxRate);
    }

    /// <summary>
    ///     Updates the tax rate
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    public virtual async Task UpdateTaxRate(TaxRate taxRate)
    {
        ArgumentNullException.ThrowIfNull(taxRate);

        await _taxRateRepository.UpdateAsync(taxRate);

        await _cacheBase.RemoveByPrefix(TAXRATE_PATTERN_KEY);

        //event notification
        await _mediator.EntityUpdated(taxRate);
    }

    #endregion
}