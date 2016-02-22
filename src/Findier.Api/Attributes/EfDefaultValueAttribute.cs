using System;

namespace Findier.Api.Attributes
{
    public class EfDefaultValueAttribute : Attribute
    {
        public EfDefaultValueAttribute()
        {
        }

        public EfDefaultValueAttribute(object value)
        {
            Value = value;
        }

        public virtual string Sql { get; set; }

        public object Value { get; set; }
    }
}