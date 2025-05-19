using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Customer.Membership.Models
{
    public class OrderViewModel
    {
        public string OrderId { get; set; }
        public int OrderNumber { get; set; }
        public double Total { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public string MembershipRole { get; set; }
    }
}