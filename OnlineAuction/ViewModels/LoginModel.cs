using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class LoginModel //: IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "Error login")]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "Error password")]        
        public string Password { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    List<ValidationResult> errors = new List<ValidationResult>();

        //    if (string.IsNullOrWhiteSpace(this.Login)) {
        //        errors.Add(new ValidationResult("Введите логин"));
        //    }
        //    if (string.IsNullOrWhiteSpace(this.Password)) {
        //        errors.Add(new ValidationResult("Введите пароль"));            
        //    }

        //    var regex = new Regex(@"[A-Za-z0-9]+", RegexOptions.Compiled);
        //    var match = regex.Match(Password);
        //    if (!(match.Success && match.Length == Password.Length)) {
        //        errors.Add(new ValidationResult("Введите корректный email", new string[] { "Email" }));
        //    }

        //    return errors;
        //}
    }
}