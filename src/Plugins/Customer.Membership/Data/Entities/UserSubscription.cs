using Grand.Domain;
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Customer.Membership.Data.Enums;

namespace Customer.Membership.Data.Entities
{
    public class UserSubscription : BaseEntity
    {

        public string UserId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ProviderCustomerId { get; set; } = string.Empty;
        public string ProviderSubscriptionId { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RenewalDate { get; set; }

        [BsonRepresentation(BsonType.String)]
        public SubscriptionStatus Status { get; set; }
        public bool IsActive { get; set; }
    }
}