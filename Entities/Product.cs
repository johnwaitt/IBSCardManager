using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities
{
    public class Product
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [Required]
        public int Year { get; set; }

        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DisplayName { get; set; } = string.Empty;

        public DateTime? ReleaseDate { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid SportId { get; set; }

        public Sport? Sport { get; set; }

        public Guid BrandId { get; set; }

        public Brand? Brand { get; set; }
    }
}