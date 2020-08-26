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

        private ICollection<Role> roles; //должна быть промеж. табл.
    
        protected Account()
        {
            roles = new List<Role>();
        }

        public Account(string Fullname, string Email, string Password) : this()
        {
            this.FullName = Fullname;
            this.Email = Email;
            this.Password = Password;

            IsActive = false;

            AddRole(new Role { RoleName = "member" });
        }
        public bool IsActive { get; set; }

        //[ForeignKey("User")]
        //public int UserId { get; protected set; }
        //public virtual User User { get; set; }

        public void AddRole(Role role)
        {
            if (roles.Contains(role))
                return;

            roles.Add(role);
        }
    }
}