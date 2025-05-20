using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Domain.Configuration;
using System.ComponentModel.DataAnnotations;
using Customer.Membership.Domain.Settings;
using Grand.Web.Models.Checkout;
using Grand.Web.Models.Common;
using PaymentMethodModel = Grand.Web.Models.Checkout.CheckoutPaymentMethodModel.PaymentMethodModel;


namespace Customer.Membership.Models
{
    // Base model for shared properties
    public class MembershipWizardModel
    {
        public int CurrentStep { get; set; } = 1;
        public string SelectedPlan { get; set; } = "";
    }

    // Step 1 Model
    public class PlanSelectionModel : MembershipWizardModel
    {
        [Required(ErrorMessage = "You must select a plan.")]
        public List<MembershipPlan> AvailablePlans { get; set; } = new();
    }

    // Step 2 Model
    public class BillingAddressModel : MembershipWizardModel
    {
        public MembershipBillingAddressModel BillingAddress { get; set; } = new();
    }

    // Step 3 Model
    public class PaymentMethodSelectionModel : MembershipWizardModel
    {
        public List<PaymentMethodModel> PaymentMethods { get; set; }
        [Required(ErrorMessage = "Please select a payment method.")]
        public string SelectedPaymentMethod { get; set; }

    }

    // Step 4 Model
    public class PaymentProcessModel : MembershipWizardModel
    {
        public string SelectedPaymentMethod { get; set; }

        public string RedirectUrl { get; set; }

        public string PaymentViewComponent { get; set; }

        public Dictionary<string, string> PaymentAdditionalData { get; set; } = new();

        public List<string> Errors { get; set; } = new();
    }
}