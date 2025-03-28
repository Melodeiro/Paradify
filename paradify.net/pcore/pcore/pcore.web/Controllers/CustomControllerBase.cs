﻿using SpotifyAPI.Web.Models;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using web.Services;

namespace web.Controllers
{
    public class CustomControllerBase : Controller
    {
        private readonly IParadifyService _paradifyService;
        private readonly ITokenCookieService _tokenCookieService;
        private readonly IUserService _userService;
        private readonly IPlaylistService _playlistService;

        public CustomControllerBase(IParadifyService paradifyService, ITokenCookieService tokenCookieService,
             IUserService userService, IPlaylistService playlistService)
        {
            _paradifyService = paradifyService;
            _tokenCookieService = tokenCookieService;
            _userService = userService;
            _playlistService = playlistService;
        }

        protected Paging<SimplePlaylist> GetPlaylists(Token token, string profileId)
        {
            var playlist = _playlistService.GetPlaylists(token, profileId);

            if (playlist != null && playlist.Items.Count == 0)
            {
                FullPlaylist fullPlaylist = _paradifyService.CreatePlaylist(profileId, "Paradify Playlist", _tokenCookieService.Get());

                if (!string.IsNullOrEmpty(fullPlaylist.Id))
                {
                    playlist = _playlistService.GetPlaylists(_tokenCookieService, profileId);
                }
            }

            return playlist;
        }

        protected PrivateProfile GetMe(CustomToken token)
        {
            return _userService.GetMe(token);
        }
    }
}