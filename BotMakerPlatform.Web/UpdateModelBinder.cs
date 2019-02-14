using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BotMakerPlatform.Web.CriticalDtos;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web
{
    public class UpdateModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;

            if (!request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                return null;

            //request.Body.Position = 0; // see: http://stackoverflow.com/a/3468653/331281
            var stream = request.Body;
            string json;

            using (var readStream = new StreamReader(stream, Encoding.UTF8))
                json = readStream.ReadToEnd();

            var values = bindingContext.ActionContext.RouteData.Values;

            var result = new WebhookUpdateDto
            {
                BotInstanceId = int.Parse(values[nameof(WebhookUpdateDto.BotInstanceId)].ToString()),
                Secret = bindingContext.ActionContext.RouteData.Values[nameof(WebhookUpdateDto.Secret)].ToString(),
                Update = JsonConvert.DeserializeObject<Update>(json)
            };

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}