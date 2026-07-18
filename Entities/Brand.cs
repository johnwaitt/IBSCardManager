using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities
{
    public class Brand
    {
        public Guid BrandId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}