namespace Findier.Api.Models.DataTransfer
{
    public class DtoThread : DtoBase
    {
        public DtoThread(DbEntry entry) : base(entry)
        {
        }

        public DtoBasicPost Post { get; set; }

        public int Unread { get; set; }

        public string User { get; set; }
    }
}