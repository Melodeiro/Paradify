namespace pcore.web
{
    public class AppSettings
    {
        public string BaseAppUrl { get; set; }
        public string ClientSecret { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string StateKey { get; set; }
        public string Scope { get; set; }
        public string SpotifyTrackUrlPattern { get; }
        public string Domain { get; set; }
        public string FullTitle { get; set; }
        public string SingleTitle { get; set; }
        public string DefaultCountryCode { get; set; }
        public string AuthorizeUrlFormat { get; set; }
        public string ResetedRefreshToken { get; set; }
    }
}
