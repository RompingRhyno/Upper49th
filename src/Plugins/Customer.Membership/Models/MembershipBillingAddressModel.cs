using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;
using Grand.Web.Common.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Customer.Membership.Models
{
    public class MembershipBillingAddressModel : BaseEntityModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string CountryId { get; set; }

        [Display(Name = "Province/State")]
        public string StateProvinceId { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address1 { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        public string ZipPostalCode { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableStates { get; set; } = new List<SelectListItem>();
    }
}
