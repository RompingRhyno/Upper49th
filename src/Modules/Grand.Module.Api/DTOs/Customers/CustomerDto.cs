﻿using Grand.Module.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Module.Api.DTOs.Customers;

public class CustomerDto : BaseApiEntityModel
{
    public Guid CustomerGuid { get; set; } = Guid.NewGuid();
    public string Username { get; set; }

    [Key] public string Email { get; set; }

    public string AdminComment { get; set; }
    public bool IsTaxExempt { get; set; }
    public bool FreeShipping { get; set; }
    public string AffiliateId { get; set; }
    public string VendorId { get; set; }
    public string StoreId { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }
    public string Gender { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Company { get; set; }
    public string StreetAddress { get; set; }
    public string StreetAddress2 { get; set; }
    public string ZipPostalCode { get; set; }
    public string City { get; set; }
    public string CountryId { get; set; }
    public string StateProvinceId { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public string VatNumber { get; set; }
    public string VatNumberStatusId { get; set; }
    public IList<string> Groups { get; set; } = new List<string>();
    public IList<AddressDto> Addresses { get; set; } = new List<AddressDto>();
}