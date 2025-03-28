﻿using SpotifyAPI.Web.Models;
using web.Services;
using web.Filters;
using web.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using pcore.web;
using Microsoft.Extensions.Options;

namespace web.Controllers
{

    public class SearchPController : CustomControllerBase
    {
        private readonly IParadifyService _paradifyService;
        private readonly ITokenCookieService _tokenCookieService;
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly IPlaylistService _playlistService;
        private readonly AppSettings Settings;

        public string _search { get; set; }
        public string _trackId { get; set; }

        public SearchPController(IParadifyService paradifyService, ITokenCookieService tokenCookieService,
            IUserService userService, ISessionService sessionService, IPlaylistService playlistService,
            IOptions<AppSettings> settings)
            : base(paradifyService, tokenCookieService,
             userService, playlistService)
        {
            _paradifyService = paradifyService;
            _tokenCookieService = tokenCookieService;
            _userService = userService;
            _sessionService = sessionService;
            _playlistService = playlistService;
            Settings = settings.Value;
        }

        [ServiceFilter(typeof(FilterClientToken))]
        public ActionResult Index(string q)
        {
            _search = q;
            _trackId = "";

            SearchResult searchResult = new SearchResult();

            if (!_search.NullCheck())
            {
                _search = _search.Decode();
               

                ViewBag.Title = string.Format("{0} - {1}", _search, Settings.SingleTitle);

                CustomToken token = ViewBag.Token;
                SearchItem searchItem = null;
                if (!token.IsTokenEmpty())
                {
                    searchItem = Search(_search, token);

                    if (searchItem != null && searchItem.Tracks != null &&
                        searchItem.Tracks.Items != null && searchItem.Tracks.Items.Count == 0)
                    {
                        var tempSearch = Regex.Replace(_search, "\\([^\\]]*\\)", "");
                        searchItem = Search(tempSearch, token);
                    }
                }
                else
                {
                    searchResult.IsTokenEmpty = true;
                }

                searchResult.SearchItem = searchItem;
                searchResult.query = _search;
                searchResult.track = _trackId;
            }

            return View("Index", searchResult);
        }

        private SearchItem Search(string query, Token token)
        {
            query = Helper.CleanQuery(query);
            return _paradifyService.SearchResult(query, token, 48);
        }

        private void SetSearchReturnUrl(string search)
        {
            var returnUrl = Helper.SetSearchReturnUrl(RouteData.Values["controller"].ToString(), search);

            _sessionService.SetReturnUrl(returnUrl);
        }
    }
}