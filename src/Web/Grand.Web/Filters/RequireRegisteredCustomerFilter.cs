using Grand.Domain.Customers;
using Grand.Infrastructure;
using Grand.Business.Core.Interfaces.Common.Directory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Grand.Web.Filters
{
    public class RequireRegisteredCustomerFilter : IAsyncActionFilter
    {
        private readonly IContextAccessor _contextAccessor;
        private readonly IGroupService _groupService;

        public RequireRegisteredCustomerFilter(IContextAccessor contextAccessor, IGroupService groupService)
        {
            _contextAccessor = contextAccessor;
            _groupService = groupService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Customer currentCustomer = _contextAccessor.WorkContext.CurrentCustomer;

            if (currentCustomer == null || await _groupService.IsGuest(currentCustomer))
            {
                context.Result = new RedirectResult("/login");
                return;
            }

            await next();
        }
    }
}
