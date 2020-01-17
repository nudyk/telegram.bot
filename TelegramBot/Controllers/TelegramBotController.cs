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
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUpdateService _updateService;
        private readonly IMapper _mapper;
        private readonly BotConfiguration _config;
        private readonly IBotService _boot;

        public TelegramBotController(ILogger<TelegramBotController> logger, IHostingEnvironment hostingEnvironment, IUpdateService updateService,
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
                if (true)
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {

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
                            }

                            questText = Regex.Replace(questText, "[\n\r]", " ").Trim();
                            if (isToThisBot)
                            {
                                var sql = string.Join(" ",
                                    "SELECT",
                                    "a.*",
                                    "FROM \"Questions\" q",
                                    "JOIN \"Answers\" a ON a.\"QuestionId\" = q.\"Id\"",
                                    $"JOIN strict_word_similarity(q.\"Text\", '{questText}') s on 1=1",
                                    "WHERE s > .4",
                                    "order by s desc");
                                var answers = db.Answers.FromSqlRaw(sql).ToList();
                                if (answers.Any())
                                {
                                    //var guid = new Guid.NewGuid();
                                    var rnd = new Random();
                                    var pos = rnd.Next(0, answers.Count);
                                    await _updateService.EchoAsync(update, answers[pos].Text);
                                }
                            }
                            else
                            {
                                var n = (new Random()).Next(10);
                                var rating = n > 6 ? ".4" : ".7";
                                var sql = string.Join(" ",
                                    "SELECT",
                                    "a.*",
                                    "FROM \"Questions\" q",
                                    "JOIN \"Answers\" a ON a.\"QuestionId\" = q.\"Id\"",
                                    $"JOIN strict_word_similarity(q.\"Text\", '{questText}') s on 1=1",
                                    $"WHERE s > {rating}",
                                    "order by s desc");
                                var answers = db.Answers.FromSqlRaw(sql).ToList();
                                if (answers.Any())
                                {
                                    //var guid = new Guid.NewGuid();
                                    var rnd = new Random();
                                    var pos = rnd.Next(0, answers.Count);
                                    await _updateService.EchoAsync(update, answers[pos].Text);
                                }
                                else
                                {
                                    _logger.LogInformation($"No any items with rating >{rating}");
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
