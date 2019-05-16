using SpotifyAPI.Web.Models;

namespace web.Models
{
    public class CustomToken : Token
    {

        public CustomToken()
        {

        }

        public CustomToken(Token token)
        {
            _Token = token;
        }
        public TokenCredentialType tokenCredentialType { get; set; }
        public Token _Token { get; }

        public enum TokenCredentialType
        {
            Auth = 1,
            Client = 2
        }
    }
}