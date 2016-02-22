namespace Findier.Api.Models.DataTransfer
{
    public class DtoPost : DtoPlainPost
    {
        public DtoPost(Post entry, string username, int ups, int downs, DtoPlainFinboard finboard) : base(entry, username, ups, downs)
        {
            Finboard = finboard;
        }

        public DtoPlainFinboard Finboard { get; set; }
    }
}