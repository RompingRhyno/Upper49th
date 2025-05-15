using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Customer.Membership.Domain.Settings;

namespace Customer.Membership.Models
{
    public class SettingsModel
    {
        public List<MembershipPlan> Plans { get; set; } = new List<MembershipPlan>();

        public List<string> AvailableRoles { get; set; }
    }
}