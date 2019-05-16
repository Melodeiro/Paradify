using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using pcore.web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using web.Services;
using static web.Models.CustomToken;

namespace web.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ITokenCookieService _tokenService;
        private readonly ISessionService _sessionService;
        public readonly AppSettings Settings;

        public CallbackController(ITokenCookieService tokenService, 
            ISessionService sessionService,
            IOptions<AppSettings> settings)
        {
            _tokenService = tokenService;
            _sessionService = sessionService;
            Settings = settings.Value;
        }

        public ActionResult Index(string code = null)
        {
            AuthorizationCodeAuth auth = new AuthorizationCodeAuth(
                Settings.RedirectUri, Settings.RedirectUri,
                Scope.None, Settings.StateKey);
            auth.ClientId = Settings.ClientId;
            auth.SecretId = Settings.ClientSecret;

            Token token = auth.ExchangeCode(code).Result;


            _tokenService.SetToken(token.AccessToken, token.RefreshToken, token.ExpiresIn, TokenCredentialType.Auth);
            _sessionService.SetToken(token.ToCustomToken(TokenCredentialType.Auth));

            bool chromeToken = Convert.ToBoolean(_sessionService.getSession("ChromeToken"));

            if (chromeToken)
            {
                _sessionService.DeleteSession("ChromeToken");

                return Redirect(string.Format("~/?access_token={0}&refresh_token={1}&expires_in={2}", token.AccessToken, token.RefreshToken, token.ExpiresIn));
            }

            if (Convert.ToBoolean(_sessionService.getSession("fromIframe")))
            {
                _sessionService.DeleteSession("fromIframe");

                return RedirectToAction("CloseIframe", "Authorize");
            }

            var returnUrl = _sessionService.GetReturnUrl();

            if (returnUrl != null && !string.IsNullOrEmpty(returnUrl))
            {
                if (HasUserAuthorized(returnUrl))
                    return Redirect("~/");
                else
                    return Redirect(returnUrl);
            }

            return Redirect("~/");
        }

        private bool HasUserAuthorized(string returnUrl)
        {
            return ((Url.RouteUrl("Authorize") != null && returnUrl.Contains(Url.RouteUrl("Authorize"))));
        }
    }
}