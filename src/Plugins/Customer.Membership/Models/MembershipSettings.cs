using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Domain.Configuration;


namespace Customer.Membership.Models
{
    public class MembershipSettings : ISettings
    {
        public string ApiKey { get; set; }
        public bool UseSandbox { get; set; }
    }
}