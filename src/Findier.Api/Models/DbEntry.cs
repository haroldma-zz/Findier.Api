using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Findier.Api.Attributes;

namespace Findier.Api.Models
{
    public abstract class DbEntry
    {
        [DateTimeNowDefaultValue]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }

    public abstract class DbEntryDeletable : DbEntry
    {
        public DateTime? DeletedAt { get; set; }
    }

    public abstract class DbEntryEditable : DbEntryDeletable
    {
        public DateTime? EditedAt { get; set; }
    }
}