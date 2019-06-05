using Microsoft.Extensions.Caching.Memory;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using web.Models;

namespace web.Services.Implementations
{
    class ParadifyService : IParadifyService
    {
        public IMemoryCache MemoryCache { get; }

        public ParadifyService(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache;
        }

        public SearchItem SearchResult(string query, Token token, int limit = 10, int offset = 0, string market = "")
        {
            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };
            try

            {

                SearchItem searchItems = api.SearchItems(query, SpotifyAPI.Web.Enums.SearchType.Track, limit);
                return searchItems;
            }
            catch
            {

            }
            return null;
        }

        public FullPlaylist CreatePlaylist(string id, string playlistName, Token token)
        {
            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };
            try
            {
                return api.CreatePlaylist(id, playlistName, false, false);
            }
            catch
            {

            }
            return null;
        }

        public Paging<SavedTrack> GetSavedTracks(CustomToken token, int limit = 10, int offset = 0, string market = "")
        {
            if (token.tokenCredentialType == CustomToken.TokenCredentialType.Client)
            {
                return null;
            }

            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };
            try
            {
                Paging<SavedTrack> savedTracks = api.GetSavedTracks(limit, offset, market);
                return savedTracks;
            }
            catch
            {

            }
            return null;
        }

        public CursorPaging<PlayHistory> GetUsersRecentlyPlayedTracks(CustomToken token, int limit = 20)
        {
            if (token.tokenCredentialType == CustomToken.TokenCredentialType.Client)
            {
                return null;
            }

            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };

            try
            {
                return api.GetUsersRecentlyPlayedTracks(limit);
            }
            catch
            {

            }
            return null;
        }

        public CustomSimpleTrack GetNewReleasedTracks(CustomToken token, string countryCode)
        {
            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };

            CustomSimpleTrack result = new CustomSimpleTrack();

            try
            {
                NewAlbumReleases newAlbumReleases = api.GetNewAlbumReleases(countryCode, limit: 5);

                if (newAlbumReleases.Albums != null && newAlbumReleases.Albums.Items != null)
                {
                    foreach (var album in newAlbumReleases.Albums.Items)
                    {
                        Paging<SimpleTrack> tracksOfTheAlbum = api.GetAlbumTracks(album.Id);

                        if (tracksOfTheAlbum.Items != null)
                        {
                            foreach (var track in tracksOfTheAlbum.Items)
                            {
                                result.TrackAlbumIds.Add(track.Id, album);

                                result.Paging.Items.Add(track);
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return result;
        }

        public Recommendations GetRecommendations(CustomToken token, string trackId, string artistId)
        {
            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };

            Recommendations recommendations = null;
            try
            {
                recommendations = api.GetRecommendations(
                         artistId.Split(',').ToList(),
                         null
                     , trackId.Split(',').ToList(), null, null, null, 18
                     );
            }
            catch
            {

            }

            return recommendations;
        }

        public PlaybackContext GetPlayingTrack(CustomToken token)
        {
            SpotifyWebAPI api = new SpotifyWebAPI()
            {
                AccessToken = token.AccessToken,
                UseAuth = true,
                TokenType = token.TokenType
            };

            try
            {
                return api.GetPlayingTrack();
            }
            catch
            {
                return null;
            }
        }

        public SeveralTracks GetTracks(List<string> tracks, Token token)
        {
            SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };
            try
            {
                return api.GetSeveralTracks(tracks);
            }
            catch
            {

            }
            return null;
        }

        public RecommendationSeedGenres GetGenres(Token token)
        {
            RecommendationSeedGenres model;

            if (!MemoryCache.TryGetValue("genres", out model))
            {
                SpotifyWebAPI api = new SpotifyWebAPI() { AccessToken = token.AccessToken, UseAuth = true, TokenType = token.TokenType };
                try
                {
                    model = api.GetRecommendationSeedsGenres();

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(new TimeSpan(1, 0, 0));

                    // Save data in cache.
                    MemoryCache.Set("genres", model, cacheEntryOptions);
                }
                catch
                {

                }
            }
            return model;
        }
    }
}