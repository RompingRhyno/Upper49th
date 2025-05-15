using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Filters
{
    public class RequireRegisteredCustomerAttribute : TypeFilterAttribute
    {
        public RequireRegisteredCustomerAttribute()
            : base(typeof(RequireRegisteredCustomerFilter))
        {
        }
    }
}
