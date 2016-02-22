using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Findier.Api.Enums;
using Findier.Api.Models.Identity;

namespace Findier.Api.Models
{
    public class Finboard : DbEntryEditable
    {
        public Country Country { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsArchived { get; set; }

        public bool IsNsfw { get; set; }

        [Required, MaxLength(45)]
        public string Slug { get; set; }

        [Required, MaxLength(100), MinLength(3)]
        public string Title { get; set; }

        public int UserId { get; set; }

        #region Navigational Properties

        public virtual User User { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        #endregion
    }
}