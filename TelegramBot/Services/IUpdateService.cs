using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DataBaseLayer;
using Telegram.Bot.Types;

namespace Telegram.Bot.Examples.DotNetCoreWebHook.Services
{
    public interface IUpdateService
    {
        Task EchoAsync(Update update, string message);

        Task AnswerCallbackQueryAsync(string callbackQueryId, string text = null, bool showAlert = false,
            string url = null, int cacheTime = 0, CancellationToken cancellationToken = default);

        Task AddLike(ApplicationContext db, Update update, User from);
        Task<Types.File> GetPhoto(PhotoSize[] msgPhoto, Stream stream);
    }
}
