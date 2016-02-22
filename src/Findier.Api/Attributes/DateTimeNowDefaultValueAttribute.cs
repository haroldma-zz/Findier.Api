using System;

namespace Findier.Api.Attributes
{
    public class DateTimeNowDefaultValueAttribute : EfDefaultValueAttribute
    {
        public override string Sql
        {
            get
            {
                return "GETUTCDATE()";
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}