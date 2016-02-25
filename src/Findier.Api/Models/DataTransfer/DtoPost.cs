namespace Findier.Api.Models.DataTransfer
{
    public class DtoPost : DtoBasicPost
    {
        public DtoPost(Post entry, string username, int ups, int downs, DtoPlainCategory category)
            : base(entry, username)
        {
            Email = entry.Email;
            PhoneNumber = entry.PhoneNumber;
            Ups = ups;
            Downs = downs;
            Category = category;
        }

        public DtoPlainCategory Category { get; set; }

        public int Downs { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public int Ups { get; set; }
    }
}