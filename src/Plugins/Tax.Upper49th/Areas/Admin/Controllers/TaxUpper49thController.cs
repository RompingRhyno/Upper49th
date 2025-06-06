﻿using Grand.Business.Core.Interfaces.Catalog.Tax;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Domain.Permissions;
using Grand.Web.Common.Controllers;
using Grand.Web.Common.DataSource;
using Grand.Web.Common.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tax.Upper49th.Domain;
using Tax.Upper49th.Models;
using Tax.Upper49th.Services;

namespace Tax.Upper49th.Areas.Admin.Controllers;

[PermissionAuthorize(PermissionSystemName.TaxSettings)]
public class TaxUpper49thController : BaseAdminPluginController
{
    private readonly ICountryService _countryService;
    private readonly IStoreService _storeService;
    private readonly ITaxCategoryService _taxCategoryService;
    private readonly ITaxRateService _taxRateService;

    public TaxUpper49thController(ITaxCategoryService taxCategoryService,
        ICountryService countryService,
        ITaxRateService taxRateService,
        IStoreService storeService)
    {
        _taxCategoryService = taxCategoryService;
        _countryService = countryService;
        _taxRateService = taxRateService;
        _storeService = storeService;
    }

    public async Task<IActionResult> Configure()
    {
        var taxCategories = await _taxCategoryService.GetAllTaxCategories();
        if (taxCategories.Count == 0)
            return Content("No tax categories can be loaded");

        var model = new TaxRateListModel();
        //stores
        model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "" });
        var stores = await _storeService.GetAllStores();
        foreach (var s in stores)
            model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id });
        //tax categories
        foreach (var tc in taxCategories)
            model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id });
        //countries
        var countries = await _countryService.GetAllCountries(showHidden: true);
        foreach (var c in countries)
            model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id });
        //states
        model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "" });
        var defaultCountry = countries.FirstOrDefault();
        if (defaultCountry != null)
        {
            var states = await _countryService.GetStateProvincesByCountryId(defaultCountry.Id);
            foreach (var s in states)
                model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id });
        }

        return View(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> RatesList(DataSourceRequest command)
    {
        var records = await _taxRateService.GetAllTaxRates(command.Page - 1, command.PageSize);
        var taxRatesModel = new List<TaxRateModel>();
        foreach (var x in records)
        {
            var m = new TaxRateModel {
                Id = x.Id,
                StoreId = x.StoreId,
                TaxCategoryId = x.TaxCategoryId,
                CountryId = x.CountryId,
                StateProvinceId = x.StateProvinceId,
                Zip = x.Zip,
                Percentage = x.Percentage
            };
            //store
            var store = await _storeService.GetStoreById(x.StoreId);
            m.StoreName = store != null ? store.Shortcut : "*";
            //tax category
            var tc = await _taxCategoryService.GetTaxCategoryById(x.TaxCategoryId);
            m.TaxCategoryName = tc != null ? tc.Name : "";
            //country
            var c = await _countryService.GetCountryById(x.CountryId);
            m.CountryName = c != null ? c.Name : "Unavailable";
            //state
            var s = c?.StateProvinces.FirstOrDefault(z => z.Id == x.StateProvinceId);
            m.StateProvinceName = s != null ? s.Name : "*";
            //zip
            m.Zip = !string.IsNullOrEmpty(x.Zip) ? x.Zip : "*";
            taxRatesModel.Add(m);
        }

        var gridModel = new DataSourceResult {
            Data = taxRatesModel,
            Total = records.TotalCount
        };

        return Json(gridModel);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> RateUpdate(TaxRateModel model)
    {
        var taxRate = await _taxRateService.GetTaxRateById(model.Id);
        taxRate.Zip = model.Zip == "*" ? null : model.Zip;
        taxRate.Percentage = model.Percentage;
        await _taxRateService.UpdateTaxRate(taxRate);

        return new JsonResult("");
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> RateDelete(string id)
    {
        var taxRate = await _taxRateService.GetTaxRateById(id);
        if (taxRate != null)
            await _taxRateService.DeleteTaxRate(taxRate);

        return new JsonResult("");
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> AddTaxRate(TaxRateListModel model)
    {
        var taxRate = new TaxRate {
            StoreId = model.AddStoreId,
            TaxCategoryId = model.AddTaxCategoryId,
            CountryId = model.AddCountryId,
            StateProvinceId = model.AddStateProvinceId,
            Zip = model.AddZip,
            Percentage = model.AddPercentage
        };
        await _taxRateService.InsertTaxRate(taxRate);

        return Json(new { Result = true });
    }
}