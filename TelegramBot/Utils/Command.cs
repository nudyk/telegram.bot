using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using DataBaseLayer;
using DataBaseLayer.Entityes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Utils
{
    public class Command
    {
        private readonly string _name;
        private readonly Func<string, ApplicationContext, TelegramUser, TelegramBotClient,
            Update, bool> _handler;
        public string Name => _name;
        
        public static Command AddAnswer = new Command("/add", CommandHandlers.AddAnswerstring);
        public static Command BotOff = new Command("/off", CommandHandlers.BotOff);
        public static Command FasAnswer = new Command("/фас", CommandHandlers.FasAnswer);
        public static Command FightAnswer = new Command("/fight", CommandHandlers.FasAnswer);
            
        
        private Command(string name, Func<string, ApplicationContext, TelegramUser, TelegramBotClient,
            Update, bool> handler)
        {
            _name = name;
            _handler = handler;
        }

        public bool Handle(string msgText, ApplicationContext db, TelegramUser @from, TelegramBotClient client,
            Update update)
        {
            if (!msgText.StartsWith(Name))
            {
                return false;
            }

            var pattern = "^" + Command.AddAnswer.Name;
            var text = Regex.Replace(msgText, pattern, string.Empty).Trim();
            return _handler(text, db, @from, client, update);
        }

        
        public static explicit operator string (Command item) {return item._name;}
        public static IEnumerable<Command> Commands => new[] {AddAnswer, BotOff, FasAnswer, FightAnswer};
    }
}
