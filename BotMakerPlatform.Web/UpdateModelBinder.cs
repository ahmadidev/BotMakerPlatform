using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using BotMakerPlatform.Web.CriticalDtos;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web
{
    public class UpdateModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;

            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                return null;

            controllerContext.HttpContext.Request.InputStream.Position = 0; // see: http://stackoverflow.com/a/3468653/331281
            var stream = controllerContext.RequestContext.HttpContext.Request.InputStream;
            string json;

            using (var readStream = new StreamReader(stream, Encoding.UTF8))
                json = readStream.ReadToEnd();

            return new WebhookUpdateDto
            {
                BotInstanceId = int.Parse(request.QueryString[nameof(WebhookUpdateDto.BotInstanceId)]),
                Secret = request.QueryString[nameof(WebhookUpdateDto.Secret)],
                Update = JsonConvert.DeserializeObject<Update>(json)
            };
        }
    }
}