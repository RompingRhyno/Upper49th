using Grand.Data;
using Customer.Membership.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Customer.Membership.Data.Entities;

namespace Customer.Membership.Data.Repositories
{
    public interface IUserSubscriptionRepository
    {
        Task<UserSubscription> GetByUserIdAsync(string userId);
        Task<List<UserSubscription>> GetActiveSubscriptionsAsync();
    }
}
