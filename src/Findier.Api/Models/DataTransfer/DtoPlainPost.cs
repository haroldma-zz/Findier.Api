namespace Findier.Api.Models.DataTransfer
{
    public class DtoPlainPost : DtoBasicPost
    {
        public DtoPlainPost(Post entry, string username, int ups, int downs) : base(entry, username)
        {
            Ups = ups;
            Downs = downs;
        }

        public int Downs { get; set; }

        public int Ups { get; set; }
    }
}