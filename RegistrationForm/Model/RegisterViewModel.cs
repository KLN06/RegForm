using System.ComponentModel.DataAnnotations;

namespace RegistrationForm.Model
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        
        public string ConfirmPassword { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string IsoCode { get; set; }

        [Required]
        [Phone]
        public string Telephone { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }
    }
}
