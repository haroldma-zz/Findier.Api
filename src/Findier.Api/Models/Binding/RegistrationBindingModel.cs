using System.ComponentModel.DataAnnotations;

namespace Findier.Api.Models.Binding
{
    public class RegistrationBindingModel
    {
        [Required, MaxLength(50)]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "Please enter an email address.")]
        [StringLength(254, ErrorMessage = "The {0} must be at least {2} characters long.")]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, MaxLength(20)]
        public string Username { get; set; }
    }
}