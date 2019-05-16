using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using pcore.web;

namespace web.IoC
{
    public class CookieManager
    {
        public readonly AppSettings Settings;
        private static string CookieDomain;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public CookieManager(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> settings)
        {
            _httpContextAccessor = httpContextAccessor;
            Settings = settings.Value;

            CookieDomain = Settings.Domain;
        }

        private static string DecodeCookieValue(string value)
        {
            return Uri.UnescapeDataString(value);
        }

        public void WriteCookie(string cookieName, string value)
        {
            WriteCookie(cookieName, value, DateTime.Now.AddDays(7));
        }

        public void WriteCookie(string cookieName, string value, DateTime expireDate)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, value, new CookieOptions()
            {
                Domain = CookieDomain,
                Expires = expireDate,
                Path = "/"
            });
        }

        public void DeleteCookie(string cookieName)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);
        }

        public bool IsCookieExist(string cookieName)
        {
            bool isCookieExist = false;

            var cookie = _httpContextAccessor.HttpContext.Request.Cookies[cookieName];

            if (cookie != null)
            {
                isCookieExist = true;
            }

            return isCookieExist;
        }

        public string GetCookieValue(string cookieName)
        {
            string cookieValue = string.Empty;

            if (IsCookieExist(cookieName))
            {
                var cookie = _httpContextAccessor.HttpContext.Request.Cookies[cookieName];
                if (cookie != null) cookieValue = cookie ?? string.Empty;
            }

            cookieValue = DecodeCookieValue(cookieValue);

            return cookieValue;
        }
    }
}
