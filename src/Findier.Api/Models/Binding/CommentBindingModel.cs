using System.ComponentModel.DataAnnotations;

namespace Findier.Api.Models.Binding
{
    public class CommentBindingModel
    {
        [Required, MaxLength(10000)]
        public string Text { get; set; }
    }
}