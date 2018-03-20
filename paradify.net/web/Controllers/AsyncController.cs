﻿using log4net;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using web.Filters;
using web.Models;
using web.Services;

namespace web.Controllers
{
    [FilterUserToken()]
    public class AsyncController : CustomControllerBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AsyncController));

        private readonly IParadifyService _paradifyService;
        private readonly ITokenCookieService _tokenCookieService;
        private readonly IHistoryService _historyService;
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly IPlaylistService _playlistService;

        public AsyncController(IParadifyService paradifyService, ITokenCookieService tokenCookieService,
            IHistoryService historyService, IUserService userService, ISessionService sessionService, IPlaylistService playlistService) : base(paradifyService, tokenCookieService,
            historyService, userService, sessionService, playlistService)
        {
            _paradifyService = paradifyService;
            _tokenCookieService = tokenCookieService;
            _historyService = historyService;
            _userService = userService;
            _sessionService = sessionService;
            _playlistService = playlistService;
        }

        [HttpGet]
        public ActionResult Recommendations(string trackId, string artistId)
        {
            CustomToken token = ViewBag.Token;

            if (token.IsTokenEmpty())
            {
                return null;
            }

            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };

            var recommendations = api.GetRecommendations(
                    artistId.Split(',').ToList(), null
                , trackId.Split(',').ToList()
                );

            if (recommendations != null && recommendations.Error != null && !string.IsNullOrEmpty(recommendations.Error.Message))
            {
                log.Error(string.Format(recommendations.Error.Message + " in Recommendations trackId:{0} artistId:{1}", trackId, artistId));
            }

            if (recommendations != null && recommendations.Tracks != null && recommendations.Tracks.Count > 0)
            {
                return PartialView("~/Views/Shared/_RecommendedSongListShort.cshtml",
               recommendations.Tracks);
            }

            return null;
        }

        [HttpGet]
        public ActionResult Playlists()
        {
            CustomToken token = ViewBag.Token;

            if (token.IsTokenEmpty())
            {
                return null;
            }

            PrivateProfile profile = GetMe(token);

            var playlists = base.GetPlaylists(token, profile.Id);

            if (playlists != null && playlists.Items != null && playlists.Items.Count > 0)
            {
                return PartialView("~/Views/Shared/_PlaylistList.cshtml", playlists.Items);
            }

            return null;
        }

        [HttpPost]
        public JsonResult Playlists(PlaylistModel model)
        {
            CustomToken token = ViewBag.Token;
            if (token.IsTokenEmpty())
            {
                return null;
            }
            PrivateProfile profile = GetMe(token);

            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, TokenType = token.TokenType };

            ErrorResponse errorResponse = api.AddPlaylistTrack(profile.Id, model.playlistId, model.trackId);

            return Json(errorResponse, JsonRequestBehavior.DenyGet);
        }

        [HttpGet]
        public ActionResult SavedTracks()
        {
            CustomToken token = ViewBag.Token;
            if (token.IsTokenEmpty())
            {
                return null;
            }
            PrivateProfile profile = GetMe(token);

            var paginSavedTracks = _paradifyService.GetSavedTracks(token, 10);

            if (paginSavedTracks != null && paginSavedTracks.Error != null && !string.IsNullOrEmpty(paginSavedTracks.Error.Message))
            {
                log.Error(paginSavedTracks.Error.Message + " in SavedTracks");
            }

            if (paginSavedTracks != null && paginSavedTracks.Items != null && paginSavedTracks.Items.Count > 0)
            {
                return PartialView("~/Views/Shared/_SavedTracksShort.cshtml", paginSavedTracks);
            }

            return null;
        }

        [HttpGet]
        public ActionResult RecentlyPlayedTracksShort()
        {
            CustomToken token = ViewBag.Token;
            if (token.IsTokenEmpty())
            {
                return null;
            }
            PrivateProfile profile = GetMe(token);

            var cursorPagingPlayHistory = _paradifyService.GetUsersRecentlyPlayedTracks(token, 10);

            if (cursorPagingPlayHistory != null && cursorPagingPlayHistory.Error != null && !string.IsNullOrEmpty(cursorPagingPlayHistory.Error.Message))
            {
                log.Error(cursorPagingPlayHistory.Error.Message + " in RecentlyPlayedTracksShort");
            }

            if (cursorPagingPlayHistory != null && cursorPagingPlayHistory.Items != null && cursorPagingPlayHistory.Items.Count > 0)
            {
                return PartialView("~/Views/Shared/_RecentlyPlayedTracksShort.cshtml", cursorPagingPlayHistory);
            }

            return null;
        }

        [HttpGet]
        public ActionResult RecentlyPlayedTracks()
        {
            CustomToken token = ViewBag.Token;
            if (token.IsTokenEmpty())
            {
                return null;
            }
            PrivateProfile profile = GetMe(token);

            var cursorPagingPlayHistory = _paradifyService.GetUsersRecentlyPlayedTracks(token, 10);

            if (cursorPagingPlayHistory != null && cursorPagingPlayHistory.Error != null && !string.IsNullOrEmpty(cursorPagingPlayHistory.Error.Message))
            {
                log.Error(cursorPagingPlayHistory.Error.Message + " in RecentlyPlayedTracks");
            }

            if (cursorPagingPlayHistory != null && cursorPagingPlayHistory.Items != null && cursorPagingPlayHistory.Items.Count > 0)
            {
                return PartialView("~/Views/Shared/_RecentlyPlayedTracks.cshtml", cursorPagingPlayHistory);
            }

            return null;
        }

        [HttpGet]
        [FilterClientToken()]
        public ActionResult GetNewReleasedTracks(string countryCode)
        {
            CustomToken customToken = ViewBag.Token;

            if (customToken.IsTokenEmpty())
            {
                return null;
            }

            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = customToken.AccessToken, UseAuth = true, TokenType = customToken.TokenType };

            CustomSimpleTrack result = new CustomSimpleTrack();

            NewAlbumReleases newAlbumReleases = api.GetNewAlbumReleases(countryCode);

            if (newAlbumReleases.Albums != null && newAlbumReleases.Albums.Items != null)
            {
                foreach (var album in newAlbumReleases.Albums.Items)
                {
                    Paging<SimpleTrack> tracksOfTheAlbum = api.GetAlbumTracks(album.Id);

                    if (tracksOfTheAlbum.Items != null)
                    {
                        Parallel.ForEach(tracksOfTheAlbum.Items, (track) =>
                        {
                            result.TrackAlbumIds.Add(track.Id, album);

                            result.Paging.Items.Add(track);
                        });

                    }
                }
            }

            return PartialView("~/Views/Shared/_NewReleasedTracks.cshtml", result);
        }

        [HttpGet]
        public ActionResult Countries()
        {
            return PartialView("~/Views/Shared/_Countries.cshtml", Constants.CountryCodes);
        }

        private PrivateProfile GetMe(CustomToken token)
        {
            return _userService.GetMe(token);
        }
    }
}