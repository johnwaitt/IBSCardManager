using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities
{
    public class Sport
    {
        public Guid SportId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string SportName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}