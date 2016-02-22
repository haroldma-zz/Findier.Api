using System.ComponentModel.DataAnnotations;

namespace Findier.Api.Models.Binding
{
    public class NewComment
    {
        /// <summary>
        ///     The comment's text content.
        /// </summary>
        [Required, MaxLength(10000)]
        public string Text { get; set; }
    }
}