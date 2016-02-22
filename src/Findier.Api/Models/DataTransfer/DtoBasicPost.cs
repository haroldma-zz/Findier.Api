using Findier.Api.Enums;

namespace Findier.Api.Models.DataTransfer
{
    public class DtoBasicPost : DtoBaseWithCreatedAt
    {
        public DtoBasicPost(Post entry, string username) : base(entry)
        {
            Title = entry.Title;
            IsArchived = entry.IsArchived;
            IsNsfw = entry.IsNsfw;
            Type = entry.Type;
            Text = entry.Text;
            User = username;
            Price = entry.Price;
        }

        public bool IsArchived { get; set; }

        public bool IsNsfw { get; set; }

        public decimal Price { get; set; }

        public string Text { get; set; }

        public string Title { get; set; }

        public PostType Type { get; set; }

        public string User { get; set; }
    }
}