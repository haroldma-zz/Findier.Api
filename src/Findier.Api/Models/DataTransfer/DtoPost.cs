namespace Findier.Api.Models.DataTransfer
{
    public class DtoPost : DtoPlainPost
    {
        public DtoPost(Post entry, string username, int ups, int downs, DtoPlainCategory category)
            : base(entry, username, ups, downs)
        {
            Category = category;
            Email = entry.Email;
            PhoneNumber = entry.PhoneNumber;
        }

        public string Email { get; set; }

        public DtoPlainCategory Category { get; set; }

        public string PhoneNumber { get; set; }
    }
}