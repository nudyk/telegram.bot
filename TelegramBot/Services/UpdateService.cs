using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataBaseLayer;
using DataBaseLayer.Entityes;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Utils;

namespace Telegram.Bot.Examples.DotNetCoreWebHook.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(IBotService botService, ILogger<UpdateService> logger)
        {
            _botService = botService;
            _logger = logger;
        }

        public async Task AnswerCallbackQueryAsync(string callbackQueryId, string text = null, bool showAlert = false, string url = null, int cacheTime = 0, CancellationToken cancellationToken = default)
        {
            await  _botService.Client.AnswerCallbackQueryAsync(callbackQueryId, text, showAlert, url, cacheTime, cancellationToken);
        }

        public async Task AddLike(ApplicationContext db, Update update, User from)
        {
            var q = update.CallbackQuery;
            var message = q.Message;
            bool isLike = q.Data == LikeConstansts.Like;
            IEnumerable<InlineKeyboardButton>[] rows = message.ReplyMarkup.InlineKeyboard.ToArray();
            List<InlineKeyboardButton> cels = rows[0].ToList();

            var query = db.Likes.Where(p => p.ChatId == message.Chat.Id && p.MessageId == message.MessageId);
            var exist = query.Where(p => p.TelegramUserId == from.Id && p.IsLike == isLike).ToList();
            if (exist.Any())
            {
                db.Likes.RemoveRange(exist);
            }
            else
            {
                exist = query.Where(p => p.TelegramUserId == from.Id && p.IsLike != isLike).ToList();
                if (exist.Any())
                {
                    db.Likes.RemoveRange(exist);
                }
                var like = new Like
                {
                    ChatId = message.Chat.Id,
                    CreatedDate = DateTime.UtcNow,
                    MessageId = message.MessageId,
                    TelegramUserId = from.Id,
                    IsLike = isLike
                };
                db.Likes.Add(like);
            }
            db.SaveChanges();
            

            var likes = query.Count(p => p.IsLike);
            var dislikes = query.Count(p => !p.IsLike);
            cels[0].Text = LikeConstansts.Like + likes;
            cels[1].Text = LikeConstansts.DizLike + dislikes;
            
            await _botService.Client.EditMessageTextAsync(message.Chat.Id, message.MessageId, message.Text,
                replyMarkup: rows);
        }

        public async Task<Types.File> GetPhoto(PhotoSize[] msgPhoto, Stream stream)
        {
            var file = msgPhoto.Last();
            var result = await _botService.Client.GetInfoAndDownloadFileAsync(file.FileId, stream);
            return result;
        }

        public async Task EchoAsync(Update update, string textForSend)
        {
            if (update.Type != UpdateType.Message && update.Type != UpdateType.InlineQuery)
                return;

            var message = update.Message;

            //_logger.LogInformation("Received Message from {0}", message.Chat.Id);
            if (update.Type == UpdateType.InlineQuery)
            {
                var uery = update.InlineQuery;
                InlineQueryResultBase[] results = {
                    // displayed result
                    new InlineQueryResultArticle(
                        id: "3",
                        title: "TgBots",
                        inputMessageContent: new InputTextMessageContent(
                            textForSend
                        )
                    )
                };
                await _botService.Client.AnswerInlineQueryAsync(uery.Id, results);
                return;
            }
            switch (message.Type)
            {
                case MessageType.Text:
                    // Echo each Message
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] // first row
                        {
                            InlineKeyboardButton.WithCallbackData(LikeConstansts.Like),
                            InlineKeyboardButton.WithCallbackData(LikeConstansts.DizLike),
                        }
                    });
                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, textForSend, replyMarkup: keyboard, replyToMessageId:message.MessageId);
                    break;

                case MessageType.Photo:
                    // Download Photo
                    var fileId = message.Photo.LastOrDefault()?.FileId;
                    var file = await _botService.Client.GetFileAsync(fileId);

                    var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                    using (var saveImageStream = System.IO.File.Open(filename, FileMode.Create))
                    {
                        await _botService.Client.DownloadFileAsync(file.FilePath, saveImageStream);
                    }

                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, "Thx for the Pics");
                    break;
            }
        }
    }
}
