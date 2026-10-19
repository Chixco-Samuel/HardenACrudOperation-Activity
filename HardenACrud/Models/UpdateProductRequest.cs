using System.ComponentModel.DataAnnotations;

namespace HardenACrud.Models
{
    public class UpdateProductRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; } = string.Empty;

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [Range(0.01, 1000000)]
        public decimal? SalePrice { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (SalePrice.HasValue && SalePrice.Value >= Price)
            {
                results.Add(new ValidationResult
                    ("Sale Price must be less than price...",
                    new[] { nameof(SalePrice) }));
            }
            return results;
        }
    }
}