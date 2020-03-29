using AutoMapper;
using DataBaseLayer.Entityes;
using Telegram.Bot.Types;

namespace TelegramBot.Utils
{
    public class MappingProfile: Profile 
    {
        public MappingProfile() 
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<TelegramUser, User>().ReverseMap();
            CreateMap<ChatInfo, Chat>().ReverseMap();
        }
    }
}
