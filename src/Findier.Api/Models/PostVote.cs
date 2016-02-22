using System.ComponentModel.DataAnnotations;
using Findier.Api.Models.Identity;

namespace Findier.Api.Models
{
    public class PostVote : DbEntry
    {
        [Required]
        public string IpAddress { get; set; }

        public bool IsUp { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        #region Navigation Properties

        public User User { get; set; }

        public Post Post { get; set; }

        #endregion
    }
}