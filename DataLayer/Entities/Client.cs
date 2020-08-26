using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DataLayer.Entities
{
    public class Client
    {
        [Key]
        public int? Id { get; set; }
        public ICollection<Order> Orders { get; set; }

        [Required]
        private decimal cash;

        [Required, Display(Name = "Accaunt")]
        [ForeignKey ("Account")]
        public  int AccountId { get; protected set; }
        public virtual Account Account { get; protected set; }

        protected Client()
        {
            Orders = new List<Order>();
        }

        public Client(Account account) : this()
        {
            Account = account ?? throw new ArgumentNullException("account");
            Account.AddRole(new Role { RoleName = "Client" });
            cash = 0;
        }

        //клиент может быть должником (услуги в кредит?)
        public bool IsDebitor()
        {
            return cash < 0 ? true : false;
        }

        //а вхоДн. сумма отрицательной?
        public void AddCash(decimal sum) {
            cash += sum;
        }

        
    }
}