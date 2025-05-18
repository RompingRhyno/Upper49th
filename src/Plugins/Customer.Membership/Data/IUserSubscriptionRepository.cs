using Grand.Data;
using Customer.Membership.Domain;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Customer.Membership.Data
{
    public interface IUserSubscriptionRepository : IRepository<UserSubscription>
    {
        Task<UserSubscription> GetByUserIdAsync(string userId);
        Task<List<UserSubscription>> GetActiveSubscriptionsAsync();
    }
}
