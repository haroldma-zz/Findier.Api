using System.ComponentModel.DataAnnotations;
using Findier.Api.Enums;

namespace Findier.Api.Models.Binding
{
    public class CreatePostBindingModel
    {
        [Required]
        public string FinboardId { get; set; }

        public bool IsNsfw { get; set; }

        public decimal Price { get; set; }

        [MaxLength(10000)]
        public string Text { get; set; }

        [Required, MaxLength(300), MinLength(10)]
        public string Title { get; set; }

        public PostType Type { get; set; }
    }
}