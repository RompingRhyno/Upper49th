using Grand.Domain.Configuration;

namespace Shipping.VendorFixedRateShipping;

public class ShippingFixedRateSettings : ISettings
{
    public int DisplayOrder { get; set; }
}