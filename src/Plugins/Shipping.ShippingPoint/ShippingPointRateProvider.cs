﻿using Grand.Business.Core.Enums.Checkout;
using Grand.Business.Core.Interfaces.Checkout.Shipping;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Business.Core.Utilities.Checkout;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Infrastructure;
using Shipping.ShippingPoint.Domain;
using Shipping.ShippingPoint.Services;
using System.Xml.Serialization;

namespace Shipping.ShippingPoint;

public class ShippingPointRateProvider : IShippingRateCalculationProvider
{
    #region Ctor

    public ShippingPointRateProvider(
        IShippingPointService shippingPointService,
        ITranslationService translationService,
        IContextAccessor contextAccessor,
        ICustomerService customerService,
        ICountryService countryService,
        ICurrencyService currencyService,
        ShippingPointRateSettings shippingPointRateSettings
    )
    {
        _shippingPointService = shippingPointService;
        _translationService = translationService;
        _contextAccessor = contextAccessor;
        _customerService = customerService;
        _countryService = countryService;
        _currencyService = currencyService;
        _shippingPointRateSettings = shippingPointRateSettings;
    }

    #endregion

    #region Fields

    private readonly IShippingPointService _shippingPointService;
    private readonly ITranslationService _translationService;
    private readonly IContextAccessor _contextAccessor;
    private readonly ICustomerService _customerService;
    private readonly ICountryService _countryService;
    private readonly ICurrencyService _currencyService;

    private readonly ShippingPointRateSettings _shippingPointRateSettings;

    #endregion

    #region Methods

    /// <summary>
    ///     Gets available shipping options
    /// </summary>
    /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
    /// <returns>Represents a response of getting shipping rate options</returns>
    public async Task<GetShippingOptionResponse> GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
    {
        ArgumentNullException.ThrowIfNull(getShippingOptionRequest);

        var response = new GetShippingOptionResponse();


        response.ShippingOptions.Add(new ShippingOption {
            Name = _translationService.GetResource("Shipping.ShippingPoint.PluginName"),
            Description = _translationService.GetResource("Shipping.ShippingPoint.PluginDescription"),
            Rate = 0,
            ShippingRateProviderSystemName = "Shipping.ShippingPoint"
        });

        return await Task.FromResult(response);
    }

    /// <summary>
    ///     Gets fixed shipping rate (if Shipping rate  method allows it and the rate can be calculated before checkout).
    /// </summary>
    /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
    /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
    public Task<double?> GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
    {
        return Task.FromResult(default(double?));
    }

    public async Task<bool> HideShipmentMethods(IList<ShoppingCartItem> cart)
    {
        return await Task.FromResult(false);
    }

    public async Task<IList<string>> ValidateShippingForm(string shippingOption, IDictionary<string, string> data)
    {
        data.TryGetValue("selectedShippingOption", out var shippingOptionId);

        var shippingMethodName = shippingOption?.Split([':'])[0];

        if (string.IsNullOrEmpty(shippingOptionId))
            return new List<string> { _translationService.GetResource("Shipping.ShippingPoint.SelectBeforeProceed") };

        if (shippingMethodName != _translationService.GetResource("Shipping.ShippingPoint.PluginName"))
            throw new ArgumentException("shippingMethodName");

        var chosenShippingOption = await _shippingPointService.GetStoreShippingPointById(shippingOptionId);
        if (chosenShippingOption == null)
            return new List<string> { _translationService.GetResource("Shipping.ShippingPoint.SelectBeforeProceed") };

        //override price 
        var offeredShippingOptions = _contextAccessor.WorkContext.CurrentCustomer.GetUserFieldFromEntity<List<ShippingOption>>(SystemCustomerFieldNames.OfferedShippingOptions, _contextAccessor.StoreContext.CurrentStore.Id);
        offeredShippingOptions.First(x => x.Name == shippingMethodName).Rate =
            await _currencyService.ConvertFromPrimaryStoreCurrency(chosenShippingOption.PickupFee,
                _contextAccessor.WorkContext.WorkingCurrency);

        await _customerService.UpdateUserField(
            _contextAccessor.WorkContext.CurrentCustomer,
            SystemCustomerFieldNames.OfferedShippingOptions,
            offeredShippingOptions,
            _contextAccessor.StoreContext.CurrentStore.Id);

        var forCustomer =
            $"<strong>{_translationService.GetResource("Shipping.ShippingPoint.Fields.ShippingPointName")}:</strong> {chosenShippingOption.ShippingPointName}<br><strong>{_translationService.GetResource("Shipping.ShippingPoint.Fields.Description")}:</strong> {chosenShippingOption.Description}<br>";

        await _customerService.UpdateUserField(
            _contextAccessor.WorkContext.CurrentCustomer,
            SystemCustomerFieldNames.ShippingOptionAttributeDescription,
            forCustomer,
            _contextAccessor.StoreContext.CurrentStore.Id);

        var serializedObject = new ShippingPointSerializable {
            Id = chosenShippingOption.Id,
            ShippingPointName = chosenShippingOption.ShippingPointName,
            Description = chosenShippingOption.Description,
            OpeningHours = chosenShippingOption.OpeningHours,
            PickupFee = chosenShippingOption.PickupFee,
            Country = (await _countryService.GetCountryById(chosenShippingOption.CountryId))?.Name,
            City = chosenShippingOption.City,
            Address1 = chosenShippingOption.Address1,
            ZipPostalCode = chosenShippingOption.ZipPostalCode,
            StoreId = chosenShippingOption.StoreId
        };

        var stringBuilder = new StringBuilder();
        string serializedAttribute;
        await using (var tw = new StringWriter(stringBuilder))
        {
            var xmlS = new XmlSerializer(typeof(ShippingPointSerializable));
            xmlS.Serialize(tw, serializedObject);
            serializedAttribute = stringBuilder.ToString();
        }

        await _customerService.UpdateUserField(_contextAccessor.WorkContext.CurrentCustomer,
            SystemCustomerFieldNames.ShippingOptionAttribute,
            serializedAttribute,
            _contextAccessor.StoreContext.CurrentStore.Id);

        return new List<string>();
    }

    public async Task<string> GetControllerRouteName()
    {
        return await Task.FromResult("Plugins.ShippingPoint.Points");
    }

    public ShippingRateCalculationType ShippingRateCalculationType => ShippingRateCalculationType.Off;

    public IShipmentTracker ShipmentTracker => null;

    public string ConfigurationUrl => ShippingPointRateDefaults.ConfigurationUrl;

    public string SystemName => ShippingPointRateDefaults.ProviderSystemName;

    public string FriendlyName => _translationService.GetResource(ShippingPointRateDefaults.FriendlyName);

    public int Priority => _shippingPointRateSettings.DisplayOrder;

    public IList<string> LimitedToStores => new List<string>();

    public IList<string> LimitedToGroups => new List<string>();

    #endregion
}