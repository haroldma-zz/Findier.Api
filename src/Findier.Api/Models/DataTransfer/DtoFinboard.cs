namespace Findier.Api.Models.DataTransfer
{
    public class DtoFinboard : DtoBase
    {
        public DtoFinboard(Finboard entry) : base(entry)
        {
            Title = entry.Title;
            Description = entry.Description;
            IsNsfw = entry.IsNsfw;
            Title = entry.Title;
        }

        public string Description { get; set; }

        public bool IsNsfw { get; set; }

        public string Title { get; set; }
    }
}