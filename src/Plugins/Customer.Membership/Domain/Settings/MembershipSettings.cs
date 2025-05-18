using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Domain.Configuration;


namespace Customer.Membership.Domain.Settings
{

    public class MembershipPlan
    {
        public string Role { get; set; }
        public decimal Price { get; set; }
        public string SystemName { get; set; }
    }
    public class MembershipSettings : ISettings
    {
        public List<MembershipPlan> Plans { get; set; } = new();
    }
}