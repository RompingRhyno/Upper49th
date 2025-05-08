using Grand.Infrastructure.ModelBinding;

namespace Shipping.VendorFixedRateShipping.Models;

public class FixedShippingRateModel
{
    public string ShippingMethodId { get; set; }

    [GrandResourceDisplayName("Plugins.Shipping.VendorFixedRateShipping.Fields.ShippingMethodName")]
    public string ShippingMethodName { get; set; }

    [GrandResourceDisplayName("Plugins.Shipping.VendorFixedRateShipping.Fields.Rate")]
    public double Rate { get; set; }
}