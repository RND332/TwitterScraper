using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TwitterScraper.Telegram
{
    interface TelegramInterface
    {
        Task AddHandler         (ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
        Task HandleSearchingAsync  (ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
        Task WriteTwittsHandler (ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
        Task WatchHandler       (ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}
