using System.ComponentModel.DataAnnotations;
using Findier.Api.Models.Identity;

namespace Findier.Api.Models
{
    public class CommentVote : DbEntry
    {
        public int CommentId { get; set; }

        [Required]
        public string IpAddress { get; set; }

        public bool IsUp { get; set; }

        public int UserId { get; set; }

        #region Navigation Properties

        public User User { get; set; }

        public Comment Comment { get; set; }

        #endregion
    }
}