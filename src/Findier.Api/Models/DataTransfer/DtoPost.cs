namespace Findier.Api.Models.DataTransfer
{
    public class DtoPost : DtoPlainPost
    {
        public DtoPost(Post entry, string username, int ups, int downs, DtoPlainFinboard finboard)
            : base(entry, username, ups, downs)
        {
            Finboard = finboard;
            Email = entry.Email;
            PhoneNumber = entry.PhoneNumber;
            CanMessage = entry.CanMessage;
        }

        public string Email { get; set; }

        public DtoPlainFinboard Finboard { get; set; }

        public string PhoneNumber { get; set; }

        public bool CanMessage { get; set; }
    }
}