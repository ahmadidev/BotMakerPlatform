using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace BotMakerPlatform.Web.Controllers
{
    public class AccountController : Controller
    {
        private IAuthenticationManager _authenticationManager;

        private IAuthenticationManager Authentication => _authenticationManager ?? (_authenticationManager = Request.GetOwinContext().Authentication);

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginWithGoogle()
        {
            return new ChallengeResult("Google", Url.Action("Index", "Home"));//Url.Action("ExternalLoginCallback", "Account"));
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return Redirect("~/");
        }
    }

    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUri)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
        }

        public string LoginProvider { get; set; }
        public string RedirectUri { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }
}