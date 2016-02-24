using System;
using System.Threading;
using Findier.Api.Models;

namespace Findier.Api.Helpers
{
    public static class LanguageHelper
    {
        public static Language GetThreadLanguage()
        {
            Language language;
            Enum.TryParse(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName, out language);
            return language;
        }
    }
}