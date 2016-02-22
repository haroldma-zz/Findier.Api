using System.ComponentModel.DataAnnotations;
using Findier.Api.Enums;

namespace Findier.Api.Models.Binding
{
    public class NewPost
    {
        /// <summary>
        ///     The finboard id.
        /// </summary>
        [Required]
        public string FinboardId { get; set; }

        /// <summary>
        ///     Is the post targeting a nsfw audience?
        /// </summary>
        public bool IsNsfw { get; set; }

        /// <summary>
        ///     The price, if the post is using the fixed type.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        ///     The post text content.
        /// </summary>
        [MaxLength(10000)]
        public string Text { get; set; }

        /// <summary>
        ///     The title of the post
        /// </summary>
        [Required, MaxLength(300), MinLength(5)]
        public string Title { get; set; }

        /// <summary>
        ///     The type of post.
        /// </summary>
        [Required]
        public PostType Type { get; set; }
    }
}