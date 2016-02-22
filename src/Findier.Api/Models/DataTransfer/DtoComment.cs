namespace Findier.Api.Models.DataTransfer
{
    public class DtoComment : DtoBaseWithCreatedAt
    {
        public DtoComment(Comment entry, string username, bool isOp, int ups, int downs) : base(entry)
        {
            Text = entry.Text;
            User = username;
            Ups = ups;
            Downs = downs;
            IsOp = isOp;
        }

        public int Downs { get; set; }

        public bool IsOp { get; set; }

        public string Text { get; set; }

        public int Ups { get; set; }

        public string User { get; set; }
    }
}