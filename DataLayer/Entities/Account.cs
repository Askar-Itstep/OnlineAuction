using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities
{
    public enum Gender { MEN, WOMEN, UNDEFINED }
    public class Account
    {
        [Key]
        public int? Id { get; set; }

        [Required, Display(Name = "Full Name")]
        [StringLength(255)]
        public string FullName { get; set; }
        public int Age { get; set; }
        [ForeignKey("Image")]
        public int ImageId { get; set; }
        public virtual Image Image { get; set; }

        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public virtual Address Address { get; set; }

        [Required, Display(Name = "Email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required, Display(Name = "Password")]
        [StringLength(255)]
        public string Password { get; set; }

        public DateTime? CreateAt { get; set; }
        public DateTime? RemoveAt { get; set; }
        private ICollection<Role> Roles { get; set; }   //-> RoleAccountLinks

        public void SetRoles(List<Role> roles)
        {
            Roles = roles;
        }

        public bool IsActive { get; set; }
        public decimal Balance { get; set; } = 0;

        public Gender Gender { get; set; }

        //-------------методы Business Logic - можно удалить----------------------
        public void AddBalance(decimal value)
        {
            Balance += value;
        }

        public void AddRole(Role role)
        {
            if (Roles.Contains(role))
            {
                return;
            }

            Roles.Add(role);
        }
        public void RemoveRole(Role role)
        {
            if (!role.RoleName.Contains("admin") && Roles != null)
            {
                if (Roles.Contains(role))
                {
                    Roles.Remove(role);
                }
            }
            return;
        }

    }
}