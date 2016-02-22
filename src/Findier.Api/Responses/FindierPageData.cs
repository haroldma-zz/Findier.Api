using System.Collections.Generic;

namespace Findier.Api.Responses
{
    public class FindierPageData<T>
    {
        public bool HasNext { get; set; }

        public bool HasPrev { get; set; }

        public List<T> Results { get; set; }
    }
}