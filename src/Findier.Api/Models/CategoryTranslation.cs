using System.ComponentModel.DataAnnotations;
using Findier.Api.Enums;

namespace Findier.Api.Models
{
    public class CategoryTranslation : DbEntry
    {
        public virtual Category Category { get; set; }

        public int CategoryId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public Language Language { get; set; }

        [Required, MaxLength(45)]
        public string Slug { get; set; }

        [Required, MaxLength(100), MinLength(3)]
        public string Title { get; set; }
    }
}