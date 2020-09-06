using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities
{
    public class Account
    {
        [Key]
        public int? Id { get; set; }

        [Required, Display(Name = "Full Name")]
        [StringLength(255)]
        public string FullName { get; set; }


        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public virtual Address Address { get; set; }

        [Required, Display(Name = "Email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required, Display(Name = "Password")]
        [StringLength(255)]
        public string Password { get; set; }

        private ICollection<Role> Roles { get; set; }   //-> RoleAccountLinks

        public void SetRoles(List<Role> roles)
        {
            Roles = roles;
        }

        public bool IsActive
        { //какая зависимость, с чем: со статистикой входа? С балансом?
            get                     //наруш. этики (сообщения)?
            {
                if (Balance < 0) { //Заменить
                    return false;
                }
                else {
                    return true;
                }
            }
            protected set { }
        }
        public decimal Balance { get; set; } = 0;

        //-------------методы Business Logic - можно удалить----------------------
        public void AddBalance(decimal value)
        {
            Balance += value;
        }

        public void AddRole(Role role)
        {
            if (Roles.Contains(role)) {
                return;
            }

            Roles.Add(role);
        }
        public void RemoveRole(Role role)
        {
            if (!role.RoleName.Contains("admin") && Roles != null) {
                if (Roles.Contains(role)) {
                    Roles.Remove(role);
                }
            }            
            return;            
        }

    }
}