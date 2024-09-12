using System.ComponentModel.DataAnnotations;

namespace RegistrationForm.Model
{
    public class RegisterViewModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Name must contain only letters.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string IsoCode { get; set; }

        [Required]
        [Phone]
        [StringLength(15, ErrorMessage ="Phone number can not be longer than 15 digits")]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "Phone number must only contain digits and cannot start with 0.")]
        public string Telephone { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }
    }
}
