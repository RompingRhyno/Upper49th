using Grand.Data.Mongo;
using Grand.Data;
using Customer.Membership.Domain;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Customer.Membership.Data
{
    public class UserSubscriptionRepository : MongoRepository<UserSubscription>, IUserSubscriptionRepository
    {
        public UserSubscriptionRepository(IAuditInfoProvider auditInfoProvider)
            : base(auditInfoProvider)
        {
        }

        public async Task<UserSubscription> GetByUserIdAsync(string userId)
        {
            return await Collection
                .Find(x => x.UserId == userId && x.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserSubscription>> GetActiveSubscriptionsAsync()
        {
            return await Collection
                .Find(x => x.IsActive)
                .ToListAsync();
        }
    }
}
