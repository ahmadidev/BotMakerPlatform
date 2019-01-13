using BotMakerPlatform.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin.Security;
using Serilog;

[assembly: OwinStartup(typeof(Startup))]

namespace BotMakerPlatform.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            IocConfig.Config();
            //SchedulerConfig.Config(app);
            LoggerConfig.Config();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                ClientId = "742244818490-kauqsvkp658r666rc92j6vsgo92u24ej.apps.googleusercontent.com",
                ClientSecret = "zqd4qkAzGZ9jcVClXsM-NgSV"
            });

            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ApplicationCookie);

            Log.Information("App started successfully.");
        }
    }
}