using SpotifyAPI.Web.Models;
using SpotifyAPI.Web;

namespace web.Services.Implementations
{
    public class UserService : IUserService
    {
        public PrivateProfile GetMe(ITokenCookieService tokenCookieService)
        {
            Token token = tokenCookieService.Get();

            return GetMe(token);
        }

        public PrivateProfile GetMe(Token token)
        {
            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, TokenType = token.TokenType };

            return api.GetPrivateProfile();
        }

        public bool Signout(ITokenCookieService tokenCookieService)
        {
            return tokenCookieService.Signout();
        }
    }
}