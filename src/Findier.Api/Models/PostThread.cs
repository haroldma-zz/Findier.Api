using System.Collections.Generic;
using Findier.Api.Models.Identity;

namespace Findier.Api.Models
{
    public class PostThread : DbEntry
    {
        public bool IsPostUserDeleted { get; set; }

        public bool IsUserDeleted { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        #region Navigational Properties

        public virtual ICollection<ThreadMessage> Messages { get; set; }

        public virtual Post Post { get; set; }

        public virtual User User { get; set; }

        #endregion
    }
}