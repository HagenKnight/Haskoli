using Microsoft.AspNetCore.Identity;

namespace Haskoli.Infrastructure.Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        /*
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public string AboutMe { get; set; } = string.Empty;
        */
    }
}
