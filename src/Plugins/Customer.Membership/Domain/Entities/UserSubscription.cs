using Grand.Domain;
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Customer.Membership.Domain.Enums;

namespace Customer.Membership.Domain
{
    public class UserSubscription : BaseEntity
    {
        public string userId { get; set; }
        public string PlanId { get; set; }

        public string Provider { get; set; } // Stripe, PayPal, etc.
        public string ProviderCustomerId { get; set; }
        public string ProviderSubscriptionId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RenewalDate { get; set; }

        [BsonRepresentation(BsonType.String)]
        public SubscriptionStatus Status { get; set; }
        public bool IsActive { get; set; }
    }
}