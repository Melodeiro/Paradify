using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using pcore.web;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using web.Models;
using web.Services;

namespace web.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly ITokenCookieService _tokenService;
        public readonly AppSettings Settings;

        public PlaylistController(ITokenCookieService tokenService, IOptions<AppSettings> settings)
        {
            _tokenService = tokenService;
            Settings = settings.Value;
        }

        [HttpPost]
        public ErrorResponse Post(PlaylistModel model)
        {
            Token token = GetToken();

            if (token.RefreshToken != null)
            {
                string oldRefreshToken = token.RefreshToken;
                token = RefreshToken(token.RefreshToken);
                token.RefreshToken = oldRefreshToken;
                CustomToken customToken = new CustomToken(token);
                _tokenService.SetToken(customToken);
            }

            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, TokenType = token.TokenType };
            ErrorResponse errorResponse = api.AddPlaylistTrack(model.playlistId, model.trackId);

            return errorResponse;
        }

        private Token RefreshToken(string refreshToken)
        {
            AuthorizationCodeAuth auth = new AuthorizationCodeAuth(
               Settings.ClientId,
               Settings.ClientSecret,
               Settings.RedirectUri,
               Settings.RedirectUri,
               Scope.None,
               Settings.StateKey
               );

            return auth.RefreshToken(refreshToken).Result;
        }

        private Token GetToken()
        {
            return _tokenService.Get();
        }

        private PrivateProfile GetMe(Token token)
        {
            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, TokenType = token.TokenType };

            PrivateProfile profile = api.GetPrivateProfile();
            return profile;
        }
    }
}
