using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineAuction.Entities
{
    public class Account
    {
        [Key]
        public int? Id { get; set; }
        private ICollection<Role> roles; //должна быть промеж. табл.
    
        protected Account()
        {
            roles = new List<Role>();
        }

        public Account(User user) : this()
        {
            User = user;
            IsActive = false;

            AddRole(new Role { RoleName = "member" });
        }
        public bool IsActive { get; set; }

        [ForeignKey("User")]
        public int UserId { get; protected set; }
        public virtual User User { get; set; }

        public void AddRole(Role role)
        {
            if (roles.Contains(role))
                return;

            roles.Add(role);
        }
    }
}