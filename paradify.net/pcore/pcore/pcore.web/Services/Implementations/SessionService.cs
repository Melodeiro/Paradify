using Microsoft.AspNetCore.Http;
using pcore.web;
using web.Models;

namespace web.Services.Implementations
{
    public class SessionService : ISessionService
    {
        public IHttpContextAccessor _httpContextAccessor { get; }

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetReturnUrl(string url)
        {
            _httpContextAccessor.HttpContext.Session.SetString("returnUrl", url);
        }

        public string GetReturnUrl()
        {
            var session = _httpContextAccessor.HttpContext.Session.GetString("returnUrl");

            return session == null ? "" : session.ToString();
        }

        public string GetResetedRefreshToken()
        {
            var session = _httpContextAccessor.HttpContext.Session.GetString("resetedRefreshToken");

            return session?.ToString();
        }

        public void SetResetedRefreshToken(string value)
        {
            _httpContextAccessor.HttpContext.Session.SetString("resetedRefreshToken", value);
        }

        public CustomToken GetToken()
        {
            CustomToken token = _httpContextAccessor.HttpContext.Session.GetObject<CustomToken>("token");

            return token;
        }

        public void SetToken(CustomToken token)
        {
           _httpContextAccessor.HttpContext.Session.SetObject("token", token);
        }

        public void DeleteToken()
        {
            _httpContextAccessor.HttpContext.Session.SetObject("token", null);
        }

        public string getSession(string key)
        {
            var sessionValue = _httpContextAccessor.HttpContext.Session.GetString(key);
    
            return sessionValue;
        }

        public void DeleteSession(string key)
        {
            _httpContextAccessor.HttpContext.Session.Remove(key);
        }

        public void SetBoolean(string key, bool value)
        {
            _httpContextAccessor.HttpContext.Session.SetObject(key, value);
        }
    }
}