namespace Findier.Api.Models.DataTransfer
{
    public class DtoPlainCategory
    {
        public DtoPlainCategory(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public string Id { get; set; }
        public string Title { get; set; }
    }
}