using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using web.Filters;
using web.Models;
using web.Services;
using Microsoft.AspNetCore.Http;

namespace web.Controllers
{
    public class AsyncController : CustomControllerBase
    {
        private readonly IParadifyService _paradifyService;
        private readonly ITokenCookieService _tokenCookieService;
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly IPlaylistService _playlistService;

        public IHttpContextAccessor _httpContextAccessor { get; }

        public AsyncController(IParadifyService paradifyService, ITokenCookieService tokenCookieService,
             IUserService userService, ISessionService sessionService, IPlaylistService playlistService)
             : base(paradifyService, tokenCookieService,
             userService, playlistService)
        {
            _paradifyService = paradifyService;
            _tokenCookieService = tokenCookieService;
            _userService = userService;
            _sessionService = sessionService;
            _playlistService = playlistService;
        }

        [HttpGet]
        [ServiceFilter(typeof(FilterClientToken))]
        public ActionResult Recommendations(string trackId, string artistId)
        {
            CustomToken token = ViewBag.Token;

            if (token.IsTokenEmpty())
            {
                return null;
            }

            var recommendations = _paradifyService.GetRecommendations(token, trackId, artistId);

            if (recommendations != null && recommendations.Tracks != null && recommendations.Tracks.Count > 0)
            {
                SeveralTracks tracks = _paradifyService.GetTracks(recommendations.Tracks.Select(t => t.Id).ToList(), token);

                if (tracks != null && tracks.Tracks != null && tracks.Tracks.Count > 0)
                {
                    return PartialView("~/Views/Shared/_RecommendedSongListShort.cshtml",
                    tracks.Tracks);
                }

            }

            return PartialView("~/Views/Shared/_RecommendedSongListShort.cshtml",
                    null);
        }

        [HttpGet]
        [ServiceFilter(typeof(FilterUserToken))]
        public ActionResult Playlists()
        {
            CustomToken token = ViewBag.Token;

            if (token.IsTokenEmpty())
            {
                return PartialView("~/Views/Shared/_LoginMessage.cshtml");
            }

            PrivateProfile profile = GetMe(token);

            if (profile == null || profile.Id == null)
            {
                return null;
            }

            var playlists = base.GetPlaylists(token, profile.Id);

            if (playlists != null && playlists.Items != null && playlists.Items.Count > 0)
            {
                return PartialView("~/Views/Shared/_PlaylistList.cshtml", playlists.Items);
            }

            return null;
        }

        [HttpPost]
        [ServiceFilter(typeof(FilterUserToken))]
        public JsonResult Playlists(PlaylistModel model)
        {
            CustomToken token = ViewBag.Token;
            if (token.IsTokenEmpty())
            {
                return null;
            }

            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, TokenType = token.TokenType };

            var tracksIds = model.trackId.Split(',');

            List<ErrorResponse> errorResponses = new List<ErrorResponse>();

            foreach (var tracksId in tracksIds)
            {
                errorResponses.Add(api.AddPlaylistTrack(model.playlistId, string.Format("spotify:track:{0}", tracksId)));
            }

            return Json(errorResponses.FirstOrDefault(e => e.Error == null));
        }

        [HttpGet]
        [ServiceFilter(typeof(FilterUserToken))]
        public ActionResult SavedTracks()
        {
            CustomToken token = ViewBag.Token;
            if (token.IsTokenEmpty())
            {
                return null;
            }

            var paginSavedTracks = _paradifyService.GetSavedTracks(token, 10);

            if (paginSavedTracks != null && paginSavedTracks.Items != null && paginSavedTracks.Items.Count > 0)
            {
                return PartialView("~/Views/Shared/_SavedTracksShort.cshtml", paginSavedTracks);
            }

            return null;
        }

        [HttpGet]
        [ServiceFilter(typeof(FilterUserToken))]
        public ActionResult RecentlyPlayedTracksShort()
        {
            CustomToken token = ViewBag.Token;
            if (token.IsTokenEmpty())
            {
                return null;
            }

            var cursorPagingPlayHistory = _paradifyService.GetUsersRecentlyPlayedTracks(token, 10);

            if (cursorPagingPlayHistory != null && cursorPagingPlayHistory.Items != null && cursorPagingPlayHistory.Items.Count > 0)
            {
                return PartialView("~/Views/Shared/_RecentlyPlayedTracksShort.cshtml", cursorPagingPlayHistory);
            }

            return null;
        }

        [HttpGet]
        [ServiceFilter(typeof(FilterUserToken))]
        public ActionResult RecentlyPlayedTracks()
        {
            CustomToken token = ViewBag.Token;
            if (token.IsTokenEmpty())
            {
                return null;
            }

            var cursorPagingPlayHistory = _paradifyService.GetUsersRecentlyPlayedTracks(token, 10);

            if (cursorPagingPlayHistory != null && cursorPagingPlayHistory.Items != null && cursorPagingPlayHistory.Items.Count > 0)
            {
                return PartialView("~/Views/Shared/_RecentlyPlayedTracks.cshtml", cursorPagingPlayHistory);
            }

            return null;
        }

        [HttpGet]
        [ServiceFilter(typeof(FilterClientToken))]
        public ActionResult GetNewReleasedTracks(string countryCode)
        {
            CustomToken token = ViewBag.Token;

            if (token.IsTokenEmpty())
            {
                return null;
            }

            var result = _paradifyService.GetNewReleasedTracks(token, countryCode);

            return PartialView("~/Views/Shared/_NewReleasedTracks.cshtml", result);
        }

        [HttpGet]
        [ServiceFilter(typeof(FilterUserToken))]
        public ActionResult GetPlayingTrack()
        {
            CustomToken token = ViewBag.Token;

            return PartialView("~/Views/Shared/PlayingTrack/_PlayingTrack.cshtml"
                , token.IsTokenEmpty() ? null : _paradifyService.GetPlayingTrack(token));
        }
    }
}