
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using pcore.web;
using web.Services;

namespace web.Controllers
{
    public class AuthorizeController : Controller
    {
        public readonly AppSettings Settings;
        private readonly ISessionService _sessionService;
        public readonly ITokenCookieService _tokenCookieService;
        public AuthorizeController(ISessionService sessionService, ITokenCookieService tokenCookieService, IOptions<AppSettings> settings)
        {
            _sessionService = sessionService;
            _tokenCookieService = tokenCookieService;
            Settings = settings.Value;
        }



        public ActionResult Index(string url, bool? fromIFrame)
        {
            if (fromIFrame.HasValue && fromIFrame.Value)
            {
                _sessionService.SetBoolean("fromIframe", fromIFrame.Value);
            }

            if (!string.IsNullOrEmpty(url))
                _sessionService.SetReturnUrl(url);
            else
                if (Request.Headers["Referer"].ToString() != null)
                _sessionService.SetReturnUrl(Request.Headers["Referer"].ToString());

            return
                Redirect(
                    string.Format(Settings.AuthorizeUrlFormat,
                    Settings.ClientId, System.Net.WebUtility.UrlEncode(Settings.RedirectUri), Settings.Scope, Settings.StateKey)
                    );
        }

        public ActionResult Logout()
        {
            _tokenCookieService.Signout();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult CloseIframe()
        {
            return View();
        }
    }
}