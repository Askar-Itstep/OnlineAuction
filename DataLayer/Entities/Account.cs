using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DataLayer.Entities
{
    public class Account
    {
        [Key]
        public int? Id { get; set; }

        [Required, Display(Name = "Full Name")]
        [StringLength(255)]
        public string FullName { get; set; }

        [Required, Display(Name = "Email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required, Display(Name = "Password")]
        [StringLength(255)]
        public string Password { get; set; }

        private ICollection<Role> Roles { get; set; }   //-> RoleAccountLinks
        
        public bool IsActive { get; set; }

        public void AddRole(Role role)
        {
            if (Roles.Contains(role))
                return;

            Roles.Add(role);
        }
    }
}