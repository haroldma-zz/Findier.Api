using System.ComponentModel.DataAnnotations;
using Findier.Api.Models.Identity;

namespace Findier.Api.Models
{
    public class ThreadMessage : DbEntry
    {
        public bool IsRead { get; set; }

        [Required, MaxLength(10000)]
        public string Text { get; set; }

        public int ThreadId { get; set; }

        public int UserId { get; set; }

        #region Navigational Properties

        public virtual PostThread Thread { get; set; }

        public virtual User User { get; set; }

        #endregion
    }
}