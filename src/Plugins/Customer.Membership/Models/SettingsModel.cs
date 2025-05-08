using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Customer.Membership.Models
{
    public class SettingsModel
    {
        public string ApiKey { get; set; }
        public bool UseSandbox { get; set; }
    }
}