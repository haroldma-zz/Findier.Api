namespace Findier.Api.Responses
{
    public class FindierErrorResponse
    {
        public string Error { get; set; }
    }

    public class FindierResponse<T>
    {
        public T Data { get; set; }
    }
}