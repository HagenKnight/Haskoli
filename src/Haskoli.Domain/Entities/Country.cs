using Haskoli.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Haskoli.Domain.Entities
{
    public class Country : EntityBase<int>
    {
        [StringLength(100)]
        public string NameEs { get; set; }
        [StringLength(100)]
        public string NameEn { get; set; }
        [StringLength(2)]
        public string ISO2 { get; set; }
        [StringLength(3)]
        public string ISO3 { get; set; }

        // Navigation property
    }
}
