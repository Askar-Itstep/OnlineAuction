using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class RoleAccountLinkVM
    {
        public int Id { get; set; }
        
        public int RoleId { get; set; }
        public virtual RoleVM Role { get; set; }
        
        public int AccountId { get; set; }
        public virtual AccountVM Account { get; set; }
    }
}