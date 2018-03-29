using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Requests;
using Telegram.Bot.Responses;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using File = Telegram.Bot.Types.File;

namespace BotMakerPlatform.Web
{
    public class SimulatorBotClient : ITelegramBotClient
    {
        private string Token { get; }
        private HttpClient HttpClient => new HttpClient();
        private const string SimulatorBaseUrl = "http://localhost:5852";

        public SimulatorBotClient(string token)
        {
            Token = token;
        }

        public Task<TResponse> MakeRequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken()) where TResponse : IResponse
        {
            throw new NotImplementedException();
        }

        public Task<bool> TestApiAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void StartReceiving(UpdateType[] allowedUpdates = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void StopReceiving()
        {
            throw new NotImplementedException();
        }

        public Task<Update[]> GetUpdatesAsync(int offset = 0, int limit = 100, int timeout = 0, UpdateType[] allowedUpdates = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task SetWebhookAsync(string url = "", FileToSend? certificate = null, int maxConnections = 40,
            UpdateType[] allowedUpdates = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var pairs = new[]
            {
                new KeyValuePair<string, string>("url", url),
                new KeyValuePair<string, string>("token", Token)
            };
            var encodedContent = new FormUrlEncodedContent(pairs);

            return HttpClient.PostAsync($"{SimulatorBaseUrl}/api/Bots/SetWebhook", encodedContent);
        }

        public Task<bool> DeleteWebhookAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<WebhookInfo> GetWebhookInfoAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = HttpClient.GetAsync($"{SimulatorBaseUrl}/api/Bots/GetWebhookInfo?token={Token}").Result;

            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException("Invalid response.");

            return Task.FromResult(JsonConvert.DeserializeObject<WebhookInfo>(result.Content.ReadAsStringAsync().Result));
        }

        public Task<User> GetMeAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = HttpClient.GetAsync($"{SimulatorBaseUrl}/api/Bots/GetMe?token={Token}").Result;

            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException("Invalid response.");

            return Task.FromResult(JsonConvert.DeserializeObject<User>(result.Content.ReadAsStringAsync().Result));
        }

        public Task<Message> SendTextMessageAsync(ChatId chatId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false,
            bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> ForwardMessageAsync(ChatId chatId, ChatId fromChatId, int messageId, bool disableNotification = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendPhotoAsync(ChatId chatId, FileToSend photo, string caption = "", bool disableNotification = false,
            int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendAudioAsync(ChatId chatId, FileToSend audio, string caption, int duration, string performer, string title,
            bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendDocumentAsync(ChatId chatId, FileToSend document, string caption = "", bool disableNotification = false,
            int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendStickerAsync(ChatId chatId, FileToSend sticker, bool disableNotification = false, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendVideoAsync(ChatId chatId, FileToSend video, int duration = 0, int width = 0, int height = 0,
            string caption = "", bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendVoiceAsync(ChatId chatId, FileToSend voice, string caption = "", int duration = 0,
            bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendVideoNoteAsync(ChatId chatId, FileToSend videoNote, int duration = 0, int length = 0,
            bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message[]> SendMediaGroupAsync(ChatId chatId, IEnumerable<InputMediaBase> media, bool disableNotification = false, int replyToMessageId = 0,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendLocationAsync(ChatId chatId, float latitude, float longitude, int livePeriod = 0,
            bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendVenueAsync(ChatId chatId, float latitude, float longitude, string title, string address,
            string foursquareId = null, bool disableNotification = false, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendContactAsync(ChatId chatId, string phoneNumber, string firstName, string lastName = null,
            bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task SendChatActionAsync(ChatId chatId, ChatAction chatAction,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<UserProfilePhotos> GetUserProfilePhotosAsync(int userId, int? offset = null, int limit = 100,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<File> GetFileAsync(string fileId, Stream destination = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> KickChatMemberAsync(ChatId chatId, int userId, DateTime untilDate = new DateTime(),
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> LeaveChatAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnbanChatMemberAsync(ChatId chatId, int userId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Chat> GetChatAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<ChatMember[]> GetChatAdministratorsAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<int> GetChatMembersCountAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<ChatMember> GetChatMemberAsync(ChatId chatId, int userId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnswerCallbackQueryAsync(string callbackQueryId, string text = null, bool showAlert = false, string url = null,
            int cacheTime = 0, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> RestrictChatMemberAsync(ChatId chatId, int userId, DateTime untilDate = new DateTime(),
            bool? canSendMessages = null, bool? canSendMediaMessages = null, bool? canSendOtherMessages = null,
            bool? canAddWebPagePreviews = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> PromoteChatMemberAsync(ChatId chatId, int userId, bool? canChangeInfo = null, bool? canPostMessages = null,
            bool? canEditMessages = null, bool? canDeleteMessages = null, bool? canInviteUsers = null,
            bool? canRestrictMembers = null, bool? canPinMessages = null, bool? canPromoteMembers = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> EditMessageTextAsync(ChatId chatId, int messageId, string text, ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> StopMessageLiveLocationAsync(ChatId chatId, int messageId, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopMessageLiveLocationAsync(string inlineMessageId, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditInlineMessageTextAsync(string inlineMessageId, string text, ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> EditMessageCaptionAsync(ChatId chatId, int messageId, string caption, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditInlineMessageCaptionAsync(string inlineMessageId, string caption, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> EditMessageReplyMarkupAsync(ChatId chatId, int messageId, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> EditMessageLiveLocationAsync(ChatId chatId, int messageId, float latitude, float longitude,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditMessageLiveLocationAsync(string inlineMessageId, float latitude, float longitude,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditInlineMessageReplyMarkupAsync(string inlineMessageId, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMessageAsync(ChatId chatId, int messageId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnswerInlineQueryAsync(string inlineQueryId, InlineQueryResult[] results, int? cacheTime = null,
            bool isPersonal = false, string nextOffset = null, string switchPmText = null, string switchPmParameter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendInvoiceAsync(ChatId chatId, string title, string description, string payload, string providerToken,
            string startParameter, string currency, LabeledPrice[] prices, string photoUrl = null, int photoSize = 0,
            int photoWidth = 0, int photoHeight = 0, bool needName = false, bool needPhoneNumber = false,
            bool needEmail = false, bool needShippingAddress = false, bool isFlexible = false, bool disableNotification = false,
            int replyToMessageId = 0, InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(), string providerData = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnswerShippingQueryAsync(string shippingQueryId, bool ok, ShippingOption[] shippingOptions = null,
            string errorMessage = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnswerPreCheckoutQueryAsync(string preCheckoutQueryId, bool ok, string errorMessage = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendGameAsync(ChatId chatId, string gameShortName, bool disableNotification = false, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SetGameScoreAsync(int userId, int score, ChatId chatId, int messageId, bool force = false,
            bool disableEditMessage = false, bool editMessage = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<Message> SetGameScoreAsync(int userId, int score, string inlineMessageId, bool force = false,
            bool disableEditMessage = false, bool editMessage = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<GameHighScore[]> GetGameHighScoresAsync(int userId, ChatId chatId, int messageId,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<GameHighScore[]> GetGameHighScoresAsync(int userId, string inlineMessageId,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<StickerSet> GetStickerSetAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<File> UploadStickerFileAsync(int userId, FileToSend pngSticker,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateNewStickerSetAsnyc(int userId, string name, string title, FileToSend pngSticker, string emojis,
            bool isMasks = false, MaskPosition maskPosition = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddStickerToSetAsync(int userId, string name, FileToSend pngSticker, string emojis,
            MaskPosition maskPosition = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetStickerPositionInSetAsync(string sticker, int position,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteStickerFromSetAsync(string sticker, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<string> ExportChatInviteLinkAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetChatPhotoAsync(ChatId chatId, FileToSend photo, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteChatPhotoAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetChatTitleAsync(ChatId chatId, string title, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetChatDescriptionAsync(ChatId chatId, string description = "",
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> PinChatMessageAsync(ChatId chatId, int messageId, bool disableNotification = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnpinChatMessageAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetChatStickerSetAsync(ChatId chatId, string stickerSetName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteChatStickerSetAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public TimeSpan Timeout { get; set; }
        public bool IsReceiving { get; }
        public int MessageOffset { get; set; }
        public event EventHandler<ApiRequestEventArgs> MakingApiRequest;
        public event EventHandler<ApiResponseEventArgs> ApiResponseReceived;
        public event EventHandler<UpdateEventArgs> OnUpdate;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<MessageEventArgs> OnMessageEdited;
        public event EventHandler<InlineQueryEventArgs> OnInlineQuery;
        public event EventHandler<ChosenInlineResultEventArgs> OnInlineResultChosen;
        public event EventHandler<CallbackQueryEventArgs> OnCallbackQuery;
        public event EventHandler<ReceiveErrorEventArgs> OnReceiveError;
        public event EventHandler<ReceiveGeneralErrorEventArgs> OnReceiveGeneralError;
    }
}