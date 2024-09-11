using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RegistrationForm.Model
{
    public class User : IdentityUser
    {
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
