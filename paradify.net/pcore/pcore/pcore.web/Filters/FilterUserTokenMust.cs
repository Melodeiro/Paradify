using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using pcore.web;
using SpotifyAPI.Web.Models;
using web.IoC;
using web.Services;

namespace web.Filters
{

    public class FilterUserTokenMust : ActionFilterAttribute
    {
        public readonly AppSettings Settings;
        private ITokenCookieService _tokenCookieService;
        private ISessionService _sessionService;
        public IHttpContextAccessor _httpContextAccessor;

        public FilterUserTokenMust(ITokenCookieService tokenCookieService, ISessionService sessionService
            , IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> settings)
        {
            _tokenCookieService = tokenCookieService;
            _sessionService = sessionService;
            _httpContextAccessor = httpContextAccessor;
            Settings = settings.Value;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (IsForceReset())
            {
                ForceReset(context);
            }
            else
            {
                Token token = _sessionService.GetToken();

                if (token == null)
                {
                    token = _tokenCookieService.Get();
                    _sessionService.SetToken(token.ToCustomToken());
                }

                if (string.IsNullOrEmpty(token.AccessToken)
                    && string.IsNullOrEmpty(token.RefreshToken))
                {
                    _sessionService.SetReturnUrl(_httpContextAccessor.GetAbsoluteUrl());

                    RedirectToAuthorize(context);
                }
                else if (string.IsNullOrEmpty(token.AccessToken)
                    && !string.IsNullOrEmpty(token.RefreshToken))
                {
                    token = RefreshToken(token.RefreshToken, Settings.ClientSecret);

                    if (string.IsNullOrEmpty(token.AccessToken))
                    {
                        _sessionService.SetReturnUrl(_httpContextAccessor.GetAbsoluteUrl());

                        RedirectToAuthorize(context);
                    }
                    else
                    {
                        _sessionService.SetToken(token.ToCustomToken());

                        _tokenCookieService.SetToken(token.AccessToken, token.RefreshToken, token.ExpiresIn, Models.CustomToken.TokenCredentialType.Auth);
                    }
                }

                if (context.Controller is Controller controller)
                {
                    controller.ViewBag.Token = token;
                }

            }

            base.OnActionExecuting(context);
        }

        public void ForceReset(ActionExecutingContext filterContext)
        {
            _tokenCookieService.DeleteToken();

            _sessionService.DeleteToken();

            _sessionService.SetResetedRefreshToken("1");
            CookieManager cookieManager = new CookieManager(_httpContextAccessor);
            cookieManager.WriteCookie("resetedRefreshToken", "1");

            RedirectToAuthorize(filterContext);
        }

        private bool IsForceReset()
        {
            bool result = false;
            string sessionResetedRefreshToken = _sessionService.GetResetedRefreshToken();

            //if no session
            if (sessionResetedRefreshToken == null)
            {
                //check if it is in the cookie
                CookieManager cookieManager = new CookieManager(_httpContextAccessor);
                if (cookieManager.GetCookieValue("resetedRefreshToken") != Settings.ResetedRefreshToken)
                {
                    result = true;
                }
            }
            else if (sessionResetedRefreshToken != Settings.ResetedRefreshToken)
            {
                result = true;
            }

            return result;
        }

        private void RedirectToAuthorize(ActionExecutingContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                    new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "controller", "Authorize" },
                        { "action", "Index" },
                        { "url", _httpContextAccessor.GetAbsoluteUrl() } });

        }

        private Token RefreshToken(string refreshToken, string clientSecret)
        {
            return _tokenCookieService.RefreshToken(refreshToken, clientSecret);
        }
    }
}