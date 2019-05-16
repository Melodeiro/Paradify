using System;
using SpotifyAPI.Web.Models;
using web.IoC;
using SpotifyAPI.Web.Auth;
using web.Models;
using static web.Models.CustomToken;
using Microsoft.AspNetCore.Http;
using SpotifyAPI.Web.Enums;
using Microsoft.Extensions.Options;
using pcore.web;

namespace web.Services.Implementations
{
    public class TokenCookieService : ITokenCookieService
    {
        public IHttpContextAccessor _httpContextAccessor { get; }
        public readonly CookieManager _cookieManager;
        public readonly AppSettings Settings;

        public TokenCookieService(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> settings)
        {
            _httpContextAccessor = httpContextAccessor;
            _cookieManager = new CookieManager(_httpContextAccessor);
            Settings = settings.Value;
        }

        public void SetToken(string accessToken, string refreshToken, double expiresIn, TokenCredentialType tokenCredentialType)
        {
            _cookieManager.WriteCookie("access_token", string.IsNullOrEmpty(accessToken) ? string.Empty: accessToken, DateTime.Now.AddSeconds(expiresIn).AddSeconds(-60));
            _cookieManager.WriteCookie("refresh_token", string.IsNullOrEmpty(refreshToken) ? string.Empty : refreshToken, DateTime.Now.AddYears(1));
            _cookieManager.WriteCookie("token_type", string.IsNullOrEmpty(tokenCredentialType.ToString()) ? string.Empty : tokenCredentialType.ToString(), DateTime.Now.AddYears(1));
        }

        public void DeleteToken()
        { 

            _cookieManager.DeleteCookie("access_token");
            _cookieManager.DeleteCookie("refresh_token");
            _cookieManager.DeleteCookie("token_type");
        }

        public void SetToken(CustomToken token)
        {
            SetToken(token._Token.AccessToken, token._Token.RefreshToken, token._Token.ExpiresIn, token.tokenCredentialType);
        }

        public CustomToken Get()
        {
            var token = GetTokenFromCookie();

            if (string.IsNullOrEmpty(token.AccessToken) && !string.IsNullOrEmpty(token.RefreshToken))
            {
                string oldRefreshToken = token.RefreshToken;
                token = RefreshToken(token.RefreshToken, Settings.ClientSecret);
                token.RefreshToken = oldRefreshToken;
                SetToken(token);
            }

            return token;
        }

        public CustomToken CustomClientCredentialToken()
        {
            var token = GetTokenFromCookie();

            if (string.IsNullOrEmpty(token.AccessToken) && !string.IsNullOrEmpty(token.RefreshToken))
            {
                string oldRefreshToken = token.RefreshToken;
                token = RefreshToken(token.RefreshToken, Settings.ClientSecret);
                token.RefreshToken = oldRefreshToken;
                SetToken(token);
            }

            return token;
        }

        public bool Signout()
        {
            _cookieManager.DeleteCookie("access_token");
            _cookieManager.DeleteCookie("refresh_token");
            _cookieManager.DeleteCookie("token_type");
            return true;
        }

        public CustomToken RefreshToken(string refreshToken, string clientSecret)
        {
            AuthorizationCodeAuth auth = new AuthorizationCodeAuth(
                Settings.ClientId,
                Settings.ClientSecret,
                Settings.RedirectUri,
                Settings.RedirectUri,
                Scope.None,
                Settings.StateKey
                );

            Token response;
            CustomToken result = null;
            try
            {
                response = auth.RefreshToken(refreshToken).Result;

                result = response.ToCustomToken();

                if (result != null)
                    result.RefreshToken = refreshToken;

            }
            catch (Exception)
            {


            }

            return result;
        }

        private CustomToken GetTokenFromCookie()
        {
            CustomToken token = new CustomToken
            {
                AccessToken = _cookieManager.GetCookieValue("access_token"),
                RefreshToken = _cookieManager.GetCookieValue("refresh_token"),
                TokenType = "Bearer"
            };

            TokenCredentialType type = TokenCredentialType.Auth;
            Enum.TryParse(_cookieManager.GetCookieValue("token_type"), out type);
            token.tokenCredentialType = type;

            return token;
        }
    }
}