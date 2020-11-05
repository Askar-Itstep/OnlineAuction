using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineAuction.ViewModels
{
    public enum Gender { MEN, WOMEN, UNDEFINED }
    public class AccountVM
    {
        public int? Id { get; set; }

        [Required, Display(Name = "Full Name")]
        [StringLength(255)]
        public string FullName { get; set; }
        public int Age { get; set; }

        public int ImageId { get; set; }
        public virtual ImageVM Image { get; set; }

        public int AddressId { get; set; }
        public virtual AddressVM Address { get; set; }

        [Required, Display(Name = "Email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required, Display(Name = "Password")]
        [StringLength(255)]
        public string Password { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime RemoveAt { get; set; }

        public Gender Gender { get; set; }
        private ICollection<RoleVM> Roles { get; set; }   //-> RoleAccountLinks

        public void SetRoles(List<RoleVM> roles)
        {
            Roles = roles;
        }
        public bool IsActive { get; set; }
        public decimal Balance { get; set; } = 0;

        public string GetAddress()
        {
            return Address.Region + ", " + Address.City + ", " + Address.Street + ", " + Address.House;
        }




    }
}