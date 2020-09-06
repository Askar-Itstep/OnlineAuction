using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class RoleVM
    {
        public int Id { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}