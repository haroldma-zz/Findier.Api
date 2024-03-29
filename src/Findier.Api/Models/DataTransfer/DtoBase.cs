using System;
using Findier.Api.Extensions;

namespace Findier.Api.Models.DataTransfer
{
    public class DtoBase
    {
        public DtoBase(DbEntry entry)
        {
            Id = entry.Id.ToEncodedId();
        }

        public string Id { get; set; }
    }

    public class DtoBaseWithCreatedAt : DtoBase
    {
        public DtoBaseWithCreatedAt(DbEntry entry) : base(entry)
        {
            CreatedAt = new DateTime(entry.CreatedAt.Ticks, DateTimeKind.Utc);
        }

        public DateTime CreatedAt { get; set; }
    }
}