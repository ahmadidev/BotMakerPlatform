using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace BotMakerPlatform.Web.Controllers
{
    public class AccountController : Controller
    {
        [Route("Login")]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [Route("Login")]
        [HttpPost]
        public ActionResult RedirectToGoogle()
        {
            return new ChallengeResult("Google");
        }
    }

    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider)
        {
            LoginProvider = provider;
        }

        public string LoginProvider { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties();
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }
}