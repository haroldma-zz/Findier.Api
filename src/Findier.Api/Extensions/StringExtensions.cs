using System.Text;

namespace Findier.Api.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///     Produces optional, URL-friendly version of a title, "like-this-one".
        ///     hand-tuned for speed, reflects performance refactoring contributed
        ///     by John Gietzen (user otac0n) - StackOverflow
        /// </summary>
        public static string ToUrlSlug(this string title, int maxLength = 45)
        {
            if (title == null) return null;
            
            var len = title.Length;
            var prevdash = false;
            var sb = new StringBuilder(len);

            for (var i = 0; i < len; i++)
            {
                var c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char) (c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                         c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    var prevlen = sb.Length;
                    sb.Append(c.RemapInternationalCharToAscii());
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxLength-1) break;
            }

            return prevdash ? sb.ToString().Substring(0, sb.Length - 1) : sb.ToString();
        }

        public static string RemapInternationalCharToAscii(this char c)
        {
            var s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            if ("èéêëę".Contains(s))
            {
                return "e";
            }
            if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            if ("żźž".Contains(s))
            {
                return "z";
            }
            if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            if ("ñń".Contains(s))
            {
                return "n";
            }
            if ("ýÿ".Contains(s))
            {
                return "y";
            }
            if ("ğĝ".Contains(s))
            {
                return "g";
            }
            switch (c)
            {
                case 'ř':
                    return "r";
                case 'ł':
                    return "l";
                case 'đ':
                    return "d";
                case 'ß':
                    return "ss";
                case 'Þ':
                    return "th";
                case 'ĥ':
                    return "h";
                case 'ĵ':
                    return "j";
            }
            return "";
        }
    }
}