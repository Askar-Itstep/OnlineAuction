using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class LoginModel
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "Error login")]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "Error password")]        
        public string Password { get; set; }
    }
}