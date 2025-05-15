using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Domain.Configuration;
using System.ComponentModel.DataAnnotations;
using Customer.Membership.Domain.Settings;

namespace Customer.Membership.Models
{
    public class MembershipModel
    {
        [Required]
        public string SelectedPlan { get; set; }

        public List<MembershipPlan> AvailablePlans { get; set; } = new List<MembershipPlan>();
    }
}