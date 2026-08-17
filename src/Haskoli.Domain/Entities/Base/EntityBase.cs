using Haskoli.Domain.Interfaces.Management;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haskoli.Domain.Entities.Base
{
    public abstract class EntityBase<TKey> : IEntityBase<TKey>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TKey Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        
        
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        
        [StringLength(50)]
        public string? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        
        [StringLength(50)]
        public string? DeletedBy { get; set; }
    }
}
