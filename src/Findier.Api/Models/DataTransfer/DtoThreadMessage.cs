namespace Findier.Api.Models.DataTransfer
{
    public class DtoThreadMessage : DtoBaseWithCreatedAt
    {
        public DtoThreadMessage(ThreadMessage entry, string username) : base(entry)
        {
            IsRead = entry.IsRead;
            Text = entry.Text;
            User = username;
        }

        public bool IsRead { get; set; }

        public string Text { get; set; }

        public string User { get; set; }
    }
}