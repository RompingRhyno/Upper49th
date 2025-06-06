﻿using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tax.Upper49th.Models;

public class TaxRateListModel : BaseModel
{
    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Store")]
    public string AddStoreId { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Country")]
    public string AddCountryId { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.StateProvince")]
    public string AddStateProvinceId { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Zip")]
    public string AddZip { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.TaxCategory")]
    public string AddTaxCategoryId { get; set; }

    [GrandResourceDisplayName("Plugins.Tax.Upper49th.Fields.Percentage")]
    public double AddPercentage { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableCountries { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableStates { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableTaxCategories { get; set; } = new List<SelectListItem>();

    public IList<TaxRateModel> TaxRates { get; set; } = new List<TaxRateModel>();
}