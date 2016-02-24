using System.ComponentModel.DataAnnotations;

namespace Findier.Api.Models.Binding
{
    public class NewUser
    {
        [Required, MaxLength(50)]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "Please enter an email address.")]
        [MaxLength(254, ErrorMessage = "The {0} must be at least {2} characters long.")]
        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6, ErrorMessage = "The {0} must be at least {2} characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, MaxLength(20)]
        public string Username { get; set; }
    }
}