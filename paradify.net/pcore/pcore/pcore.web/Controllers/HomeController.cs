using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using pcore.web;
using web.Filters;
using web.Services;
    
namespace web.Controllers
{
    //[ServiceFilter(typeof(FilterClientToken))]
    public class HomeController : CustomControllerBase
    {
        public readonly AppSettings Settings;

        public IHttpContextAccessor HttpContextAccessor { get; }

        public HomeController(IParadifyService paradifyService, ITokenCookieService tokenCookieService,
            IUserService userService, IPlaylistService playlistService
            , IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> settings)
            : base(paradifyService, tokenCookieService,
            userService, playlistService)
        {
            HttpContextAccessor = httpContextAccessor;
            Settings = settings.Value;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Installed()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}