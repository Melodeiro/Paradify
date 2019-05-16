using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using pcore.web;
using web.Models;
using web.Services;
using static web.Models.CustomToken;

namespace web.Filters
{
    public class FilterUserToken : ActionFilterAttribute
    {
        public readonly AppSettings Settings;
        private ITokenCookieService _tokenCookieService;
        private ISessionService _sessionService;

        public FilterUserToken(ITokenCookieService tokenCookieService, ISessionService sessionService, IOptions<AppSettings> settings)
        {
            _tokenCookieService = tokenCookieService;
            _sessionService = sessionService;
            Settings = settings.Value;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            CustomToken token = _sessionService.GetToken();

            if (token == null)
            {
                token = _tokenCookieService.Get();

                if (!string.IsNullOrEmpty(token.AccessToken)
                    && token.tokenCredentialType == TokenCredentialType.Auth)
                    _sessionService.SetToken(token.ToCustomToken(token.tokenCredentialType));
            }

            if (token == null || (string.IsNullOrEmpty(token.AccessToken)
                && !string.IsNullOrEmpty(token.RefreshToken)))
            {
                token = RefreshToken(token.RefreshToken, Settings.ClientSecret);

                if (token == null || string.IsNullOrEmpty(token.AccessToken))
                {
                    //Just Die
                }
                else
                {
                    _tokenCookieService.SetToken(token);
                    _sessionService.SetToken(token);
                }
            }

            var controller = context.Controller as Controller;

            if (controller != null)
            {
                if (token.tokenCredentialType != TokenCredentialType.Auth)
                {
                    controller.ViewBag.Token = null;
                }
                else
                    controller.ViewBag.Token = token;
            }

           

            base.OnActionExecuting(context);
        }

        private CustomToken RefreshToken(string refreshToken, string clientSecret)
        {
            return _tokenCookieService.RefreshToken(refreshToken, clientSecret);
        }
    }
}