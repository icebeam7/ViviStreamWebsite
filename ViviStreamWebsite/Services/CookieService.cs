using System;
using Microsoft.AspNetCore.Http;

namespace ViviStreamWebsite.Services
{
    public static class CookieService
    {
        public static void Set(HttpResponse response, string key, string value)
        {
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddYears(1);

            response.Cookies.Append(key, value, option);
        }

        public static string Get(HttpRequest request, string key)
        {
            return (request.Cookies.ContainsKey(key)) ? request.Cookies[key] : string.Empty;
        }
    }
}
