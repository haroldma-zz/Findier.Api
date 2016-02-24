using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using Findier.Api.Enums;
using Findier.Api.Models.Identity;

namespace Findier.Api.Models
{
    public class Post : DbEntryEditable
    {
        [EmailAddress]
        public string Email { get; set; }

        public int FinboardId { get; set; }

        public bool IsArchived { get; set; }

        public bool IsNsfw { get; set; }

        [Required]
        public DbGeography Location { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public decimal Price { get; set; }

        [Required, MaxLength(45)]
        public string Slug { get; set; }

        [MaxLength(10000)]
        public string Text { get; set; }

        [Required, MaxLength(300), MinLength(5)]
        public string Title { get; set; }

        public PostType Type { get; set; }

        public int UserId { get; set; }

        #region Navigation Properties

        public virtual User User { get; set; }

        public virtual Finboard Finboard { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<PostVote> Votes { get; set; }

        #endregion
    }
}