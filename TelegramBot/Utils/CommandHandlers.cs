using DataBaseLayer;
using DataBaseLayer.Entityes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Utils
{
    public static class CommandHandlers
    {
        public static bool AddAnswerstring(string msgText, ApplicationContext db, TelegramUser @from, TelegramBotClient client,
            Update update1)
        {
            var lines = msgText.Split('\n','\r', StringSplitOptions.RemoveEmptyEntries).Where(p => !string.IsNullOrEmpty(p));
            foreach (var line in lines)
            {
                var parameters = line.Split(";");
                if (parameters.Length < 2)
                {
                    return true;
                }

                var quest = parameters[0].Trim();
                var answer = parameters[1];
                var dbQuest = db.Questions.FirstOrDefault(p => p.Text == quest);
                if (dbQuest == null)
                {
                    dbQuest = new Question
                    {
                        Text = quest,
                        CreatedDate = DateTime.UtcNow,
                        CreatorId = from.Id,
                        Quantity = 1,
                    };
                    db.Questions.Add(dbQuest);
                }
                else
                {
                    if (db.Answers.Any(p => p.Text == answer))
                    {
                        return true;
                    }
                }

                var dbAnswer = new Answer
                {
                    Text = answer,
                };
                dbQuest.Answers.Add(dbAnswer);
                db.SaveChanges();
            }

            return true;
        }

        public static bool BotOff(string msgText, ApplicationContext db, TelegramUser @from, TelegramBotClient client,
            Update update1)
        {
            //client.DeleteWebhookAsync().Wait();
            return true;
        }
        public static bool FasAnswer(string msgText, ApplicationContext db, TelegramUser @from, TelegramBotClient client,
            Update update)
        {
            var message = update.Message;
            var textForSend = GetAnswer(db, msgText);
            Thread.Sleep(3000);
            client.SendTextMessageAsync(message.Chat.Id, textForSend);
            return true;
        }

        private static string GetAnswer(ApplicationContext db, string questText)
        {
            var sql = string.Join(" ",
                "SELECT",
                "a.*",
                "FROM \"Questions\" q",
                "JOIN \"Answers\" a ON a.\"QuestionId\" = q.\"Id\"",
                $"JOIN strict_word_similarity(q.\"Text\", '/фас') s on 1=1",
                "WHERE s > .4",
                "order by s desc");
            var answers = db.Answers.FromSqlRaw(sql).ToList();
            if (answers.Any())
            {
                //var guid = new Guid.NewGuid();
                var rnd = new Random();
                var pos = rnd.Next(0, answers.Count);
                return answers[pos].Text;
            }

            return null;
        }
    }
}
