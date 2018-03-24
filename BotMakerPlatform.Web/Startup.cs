using BotMakerPlatform.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin.Security;

[assembly: OwinStartup(typeof(Startup))]

namespace BotMakerPlatform.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            //    LoginPath = new PathString("/Login"),
            //    //Provider = new CookieAuthenticationProvider
            //    //{
            //        // Enables the application to validate the security stamp when the user logs in.
            //        // This is a security feature which is used when you change a password or add an external login to your account.  
            //        //OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
            //        //    validateInterval: TimeSpan.FromMinutes(30),
            //        //    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
            //    //}
            //});

            //TODO: Make it better, bitch.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //app.Use(async (context, next) =>
            //{
            //    await next.Invoke();
            //});

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                ClientId = "742244818490-kauqsvkp658r666rc92j6vsgo92u24ej.apps.googleusercontent.com",
                ClientSecret = "zqd4qkAzGZ9jcVClXsM-NgSV"
            });

            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ApplicationCookie);

            //app.Use(async (context, next) =>
            //{
            //    await next.Invoke();
            //});

        }
    }
}