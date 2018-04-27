using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.Http;
using BotMakerPlatform.Simulator.Controllers;
using Newtonsoft.Json;

namespace BotMakerPlatform.Simulator
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var chatsJson = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/App_Data/Chats.json"));
            ChatsController.Chats = JsonConvert.DeserializeObject<List<ChatModel>>(chatsJson);
        }
    }
}
