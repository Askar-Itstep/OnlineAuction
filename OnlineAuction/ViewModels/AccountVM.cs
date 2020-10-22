using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class AccountVM
    {
        //public string ConnectionId { get; set; }
        public int? Id { get; set; }

        [Required, Display(Name = "Full Name")]
        [StringLength(255)]
        public string FullName { get; set; }

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
        ////-------------методы Business Logic - можно удалить----------------------
        //public void AddBalance(decimal value)
        //{
        //    Balance += value;
        //}

        //public void AddRole(RoleVM role)
        //{
        //    if (Roles.Contains(role)) {
        //        return;
        //    }

        //    Roles.Add(role);
        //}
        //public void RemoveRole(RoleVM role)
        //{
        //    if (!role.RoleName.Contains("admin") && Roles != null) {
        //        if (Roles.Contains(role)) {
        //            Roles.Remove(role);
        //        }
        //    }
        //    return;
        //}

    }
}