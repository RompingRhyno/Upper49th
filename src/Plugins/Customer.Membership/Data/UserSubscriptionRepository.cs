using Grand.Data.Mongo;
using Customer.Membership.Domain;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Customer.Membership.Data
{
    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        private readonly MongoRepository<UserSubscription> _repository;

        public UserSubscriptionRepository(MongoRepository<UserSubscription> repository)
        {
            _repository = repository;
        }

        public async Task<UserSubscription> GetByUserIdAsync(string userId)
        {
            return await _repository
                .Collection
                .Find(x => x.CustomerId == userId && x.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserSubscription>> GetActiveSubscriptionsAsync()
        {
            return await _repository
                .Collection
                .Find(x => x.IsActive)
                .ToListAsync();
        }
    }
}
