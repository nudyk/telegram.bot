using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using DataBaseLayer;
using DataBaseLayer.Entityes;
using FreeImageAPI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot.Examples.DotNetCoreWebHook.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Utils;

namespace Telegram.Bot.Examples.DotNetCoreWebHook.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TelegramBotController : ControllerBase
    {
        private readonly ILogger<TelegramBotController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IUpdateService _updateService;
        private readonly IMapper _mapper;
        private readonly BotConfiguration _config;
        private readonly IBotService _boot;

        public TelegramBotController(ILogger<TelegramBotController> logger, IWebHostEnvironment hostingEnvironment, IUpdateService updateService,
            IMapper mapper, IOptions<BotConfiguration> config, IBotService boot)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _updateService = updateService;
            _mapper = mapper;
            _config = config.Value;
            _boot = boot;
        }
        [Route(@"update")]
        [HttpPost]
        public async Task<OkResult> Update([FromBody]Update update)
        {
            try
            {
                var inputData = System.Text.Json.JsonSerializer.Serialize(update);
                var msg = update.Message;
                if (msg == null)
                {
                    _logger.LogInformation("Incoming empty message");
                    Ok();
                }

                _logger.LogInformation("Incomming message from {0}, contact {1}, message {2}",
                    string.Concat(update?.Message?.From?.Id, ":", update?.Message?.From?.FirstName, " ",
                        update?.Message?.From?.LastName),
                    update?.Message?.Contact?.PhoneNumber,
                    update?.Message?.Text);
                ChatInfo chatInfo = null;
                if (true)
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        Chat chat = msg.Chat;
                        if (chat != null)
                        {
                            chatInfo = _mapper.Map<ChatInfo>(chat);
                            Save(db, chatInfo);
                        }

                        User from = GetFrom(update);
                        if (update?.Message?.Contact != null)
                        {

                        }
                        if (from == null)
                        {
                            return Ok();
                        }

                        var fromUser = _mapper.Map<TelegramUser>(from);
                        var user = db.TelegramUsers.FirstOrDefault(p => p.Id == fromUser.Id);
                        if (user == null)
                        {
                            db.TelegramUsers.Add(fromUser);
                            db.SaveChanges();
                        }

                        if (update.Type == UpdateType.CallbackQuery)
                        {
                            await _updateService.AddLike(db, update, from);
                            return Ok();
                        }

                        db.SaveChanges();

                        if (!ProcessCommand(update, msg?.Text, db, fromUser, update))
                        {
                            var questText = msg?.Text;
                            if (questText == null)
                            {
                                //if (msg?.Photo.Length > 0)
                                //{
                                //    var stream = new MemoryStream();
                                //    var types = await _updateService.GetPhoto(msg.Photo, stream);
                                //    FIBITMAP bitmap = FreeImage.LoadFromStream(stream);
                                //    BITMAPINFO imageInfo = FreeImage.GetInfoEx(bitmap);
                                //    FREE_IMAGE_FORMAT imageType = FreeImage.GetFileTypeFromStream(stream);
                                //}
                                //return Ok();
                            }
                            bool isToThisBot = questText.StartsWith(_config.BotName);
                            if (from.IsBot && questText!= null)
                            {
                                questText = Regex.Replace(questText, @"^@\S+\s", String.Empty);
                            }
                            if (update.Type == UpdateType.Message)
                            {
                                //questText = update.InlineQuery.Query;
                                var quest = db.Questions.FirstOrDefault(p => p.Text == questText);
                                if (quest == null && isToThisBot)
                                {
                                    quest = new Question
                                    {
                                        Text = questText.Substring(0, Math.Min(questText.Length, 128)),
                                        CreatedDate = DateTime.UtcNow,
                                        CreatorId = from.Id,
                                        Quantity = 1
                                    };
                                    db.Questions.Add(quest);
                                }
                                else if(quest != null)
                                {
                                    quest.UpdatedDate = DateTime.UtcNow;
                                    quest.UpdatedId = from.Id;
                                    quest.Quantity++;
                                }
                                else if (!isToThisBot && !string.IsNullOrEmpty(questText) && !string.IsNullOrEmpty( msg?.ReplyToMessage?.Text))
                                {
                                    //Add replies human to human as quest and answer
                                    string replyText = msg.ReplyToMessage.Text;
                                    replyText = Regex.Replace(replyText, "[\n\r]", " ").Trim();
                                    replyText = replyText.Substring(0, Math.Min(replyText.Length, 128));
                                    var replyQuest = db.Questions.FirstOrDefault(p => p.Text.Equals(replyText));
                                    if (replyQuest == null)
                                    {
                                        replyQuest = new Question
                                        {
                                            Text = replyText,
                                            CreatedDate = DateTime.UtcNow,
                                            CreatorId = msg.ReplyToMessage.From.Id,
                                            Quantity = 1,
                                            Answers = new List<Answer>()
                                        };
                                        db.Questions.Add(replyQuest);
                                    }
                                    else
                                    {
                                        replyQuest.UpdatedDate = DateTime.UtcNow;
                                        replyQuest.UpdatedId = msg.ReplyToMessage.From.Id;
                                        replyQuest.Quantity++;
                                    }

                                    var answeText = questText.Substring(0, Math.Min(questText.Length, 512));
                                    answeText = Regex.Replace(answeText, "[\n\r]", " ").Trim();
                                    var q = db.Answers.Where(p => p.QuestionId == 88).ToList();
                                    var answer = db.Answers.Where(p => p.Text == answeText  && p.QuestionId == replyQuest.Id).FirstOrDefault();
                                    if (answer == null)
                                    {
                                        answer = new Answer
                                        {
                                            Text = answeText,
                                        };
                                        replyQuest.Answers.Add(answer);
                                    }

                                    db.SaveChanges();
                                    return Ok();
                                }
                            }

                            questText = Regex.Replace(questText, "[\n\r]", " ").Trim();
                            string toSend = null;
                            int answerId = 0;
                            if (isToThisBot)
                            {
                                var answers = GetBotAnswers(db, questText, ".4");
                                answerId = SelectMessage(answers, answerId, ref toSend);
                            }
                            else
                            {
                                var n = (new Random()).Next(10);
                                var rating = n > 6 ? ".4" : ".7";
                                var answers = GetBotAnswers(db, questText, rating);
                                if (answers.Any())
                                {
                                    answerId = SelectMessage(answers, answerId, ref toSend);
                                }
                                else
                                {
                                    _logger.LogInformation($"No any items with rating >{rating}");
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(toSend) && (chatInfo == null || !chatInfo.IsSelentMode))
                            {
                                var responce = await _updateService.EchoAsync(update, toSend);
                                if (responce != null)
                                {
                                    var sended = new SendedAnswer
                                    {
                                        AnswerId = answerId,
                                        CreatedDate = DateTime.UtcNow,
                                        MessageId = responce.MessageId,
                                    };
                                    db.SendedAnswers.Add(sended);
                                    
                                    var inputMessage = new InputMessage
                                    {
                                        MessageId = responce.MessageId,
                                        Text = questText
                                    };
                                    db.InputMessages.Add(inputMessage);

                                    db.SaveChanges();
                                }
                            }

                        }
                    }

                    
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error");
            }

            return Ok();
        }

        private void Save(ApplicationContext db, ChatInfo chatInfo)
        {
            var exist = db.ChatInfos.FirstOrDefault(p => p.Id == chatInfo.Id);
            if (exist != null)
            {
                exist.Type = chatInfo.Type;
                exist.CanSetStickerSet = chatInfo.CanSetStickerSet;
                exist.Description = chatInfo.Description;
                exist.FirstName = chatInfo.FirstName;
                exist.InviteLink = chatInfo.InviteLink;
                exist.LastName = chatInfo.LastName;
                exist.SlowModeDelay = chatInfo.SlowModeDelay;
                exist.StickerSetName = chatInfo.StickerSetName;
                exist.Title = chatInfo.Title;
                exist.Username = chatInfo.Username;
                exist.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                chatInfo.CreatedDate = DateTime.UtcNow;
                db.ChatInfos.Add(chatInfo);
            }

            db.SaveChanges();
        }

        private static int SelectMessage(List<Answer> answers, int answerId, ref string toSend)
        {
            if (answers.Any())
            {
                //var guid = new Guid.NewGuid();
                var rnd = new Random();
                var pos = rnd.Next(0, answers.Count);
                answerId = answers[pos].Id;
                toSend = answers[pos].Text;
            }

            return answerId;
        }

        private List<Answer> GetBotAnswers(ApplicationContext db, string questText, string minRating)
        {
            int minLikeRating = -10;
            var sql = string.Join(" ",
                "SELECT",
                "a.*",
                "FROM \"Questions\" q",
                "JOIN \"Answers\" a ON a.\"QuestionId\" = q.\"Id\"",
                $"JOIN strict_word_similarity('{questText}', q.\"Text\") s on 1=1",
                "LEFT JOIN (",
                    "SELECT a.\"Id\", a.\"QuestionId\", SUM(CASE WHEN l.\"IsLike\" THEN 1 ELSE -1 END) as Rating",
                    "FROM \"Likes\" l",
                    "INNER JOIN \"SendedAnswers\" sa on sa.\"MessageId\" = l.\"MessageId\"",
                    "INNER JOIN \"Answers\" a on a.\"Id\" = sa.\"AnswerId\"",
                    "GROUP BY a.\"Id\", a.\"QuestionId\"",
                ") r on r.\"Id\" = a.\"Id\" AND r.\"QuestionId\" = a.\"QuestionId\"",
                $"WHERE s > {minRating} AND (r.Rating IS NULL OR r.Rating > ({minLikeRating}))",
                "order by s desc");
            List<Answer> answers = db.Answers.FromSqlRaw(sql).ToList();
            return answers;
        }
        private User GetFrom(Update update)
        {
            User result = null;
            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                    result = update.CallbackQuery.From;
                    break;
                case UpdateType.Message:
                    result = update.Message.From;
                    break;
                case UpdateType.EditedMessage:
                    result = update.EditedMessage.From;
                    break;
                case UpdateType.ChannelPost:
                case UpdateType.EditedChannelPost:
                    result = update.ChannelPost.From;
                    break;
                case UpdateType.ChosenInlineResult:
                    result = update.ChosenInlineResult.From;
                    break;
                case UpdateType.Poll:
                    result = update.Message.From;
                    break;
                case UpdateType.PreCheckoutQuery:
                    result = update.PreCheckoutQuery.From;
                    break;
                case UpdateType.ShippingQuery:
                    result = update.ShippingQuery.From;
                    break;
                case UpdateType.InlineQuery:
                    result = update.InlineQuery.From;
                    break;
            }

            return result;
        }

        private bool ProcessCommand(Update update, string msgText, ApplicationContext db, TelegramUser @from,
            Update update1)
        {
            _logger.LogInformation("Try Process command {0}", msgText);
            if (string.IsNullOrEmpty(msgText))
            {
                return false;
            }
            foreach (var command in Command.Commands)
            {
                if (msgText.StartsWith(command.Name))
                {
                    var result =  command.Handle(msgText, db, from, _boot.Client, update);
                    if (result)
                    {
                        //_updateService.EchoAsync(update, "Принято");
                    }
                    return result;
                }
            }

            return false;
        }
    }
}
