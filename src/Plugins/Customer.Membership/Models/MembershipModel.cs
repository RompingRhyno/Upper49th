using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Domain.Configuration;
using System.ComponentModel.DataAnnotations;
using Customer.Membership.Domain.Settings;
using Grand.Web.Models.Checkout;
using PaymentMethodModel = Grand.Web.Models.Checkout.CheckoutPaymentMethodModel.PaymentMethodModel;

namespace Customer.Membership.Models
{
    public class MembershipModel
    {

        public int CurrentStep { get; set; } = 1;

        // For selecting plan (step 1)
        [Required(ErrorMessage = "You must select a plan.")]
        public string SelectedPlan { get; set; } = "";
        public List<MembershipPlan> AvailablePlans { get; set; } = new List<MembershipPlan>();

        // Honestly probably need billing address first as step 2 and then method as step 3 (to get billing address)
        // TO-DO add real step 2 billing address

        // For payment (step 2)
        public List<PaymentMethodModel> PaymentMethods { get; set; }
        public string SelectedPaymentMethod { get; set; }
    }
}