using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Findier.Api.Models.Identity;

namespace Findier.Api.Models
{
    public class Comment : DbEntryEditable
    {
        public int PostId { get; set; }

        [MaxLength(10000)]
        public string Text { get; set; }

        public int UserId { get; set; }

        #region Navigation Properties

        public virtual User User { get; set; }

        public virtual Post Post { get; set; }

        public virtual ICollection<CommentVote> Votes { get; set; }

        #endregion
    }
}