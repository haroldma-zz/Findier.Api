using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Findier.Api.Attributes;
using Findier.Api.Enums;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Findier.Api.Models.Identity
{
    public class User : IdentityUser<int, AppUserLogin, AppUserRole, AppUserClaim>
    {
        public Country Country { get; set; }

        [DateTimeNowDefaultValue, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [Required, MaxLength(50), MinLength(5)]
        public string DisplayName { get; set; }

        [Required, EmailAddress, MaxLength(254), Index(IsUnique = true)]
        public override string Email { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }

        [Required, EmailAddress, MaxLength(254), Index(IsUnique = true)]
        public string NormalizedEmail { get; set; }

        [Required, MaxLength(20), Index(IsUnique = true)]
        public string NormalizedUserName { get; set; }

        [Required, MaxLength(20), Index(IsUnique = true)]
        public override string UserName { get; set; }

        #region Navigational Properties

        public virtual ICollection<ThreadMessage> SentMessages { get; set; }

        public virtual ICollection<PostThread> StartedThreads { get; set; }

        public virtual ICollection<Finboard> Finboards { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        public virtual ICollection<PostVote> PostVotes { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<CommentVote> CommentVotes { get; set; }

        #endregion
    }
}