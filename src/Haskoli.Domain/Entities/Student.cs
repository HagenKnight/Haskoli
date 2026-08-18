using Haskoli.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Haskoli.Domain.Entities
{
    public class Student : EntityBase<int>
    {
        [StringLength(20)]
        public string Document { get; set; } = string.Empty;
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // Navigation property
    }
}
