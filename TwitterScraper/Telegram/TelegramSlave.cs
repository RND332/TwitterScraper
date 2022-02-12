using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TwitterScraper.GoogleAPI;
using TwitterScraper.NitterAPI;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using System.Collections.Concurrent;

namespace TwitterScraper.Telegram
{
    internal class TelegramSlave : TelegramInterface
    {
        private static ConcurrentDictionary<string, string> CheckingList = new ConcurrentDictionary<string, string>();

        TablesSlave Slave = new TablesSlave(new GoogleClient());
        bool IsCheckerRunning = false;
        public TelegramBotClient Bot { get; private set; }

        private static long ChatId;
        public TelegramSlave(string API) 
        {
            Slave = new TablesSlave(new GoogleClient());
            Bot = new TelegramBotClient(API);

            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cts.Token);

            Console.ReadLine();

        }
        public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) 
        {
            Task action = null;
            if (update.Message != null && update.Message.Text != null)
            {
                action = update.Message.Text!.Split(' ')[0].Replace("@DTFRNDSSPbot", "").ToLower() switch
                {
                    "/add" => AddHandler(botClient, update, cancellationToken),
                    "/updatepost" => UpdatePostHandler(botClient, update, cancellationToken),
                    "/watch" => WatchHandler(botClient, update, cancellationToken),
                    "/writetwitts" => WriteTwittsHandler(botClient, update, cancellationToken),
                    "/search" => HandleSearchingAsync(botClient, update, cancellationToken),
                };
            }

            try
            {
                if (action != null)await action;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Catched Exception from Telegram: " + exception);
            }
        }
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        public async Task AddHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var link = update.Message.Text!.Split(' ')[1];

            if (link != String.Empty)
            {
                await botClient.SendTextMessageAsync(
                    chatId: ChatId,
                    text: $"Will add {link} to google docs",
                    cancellationToken: cancellationToken);

                var Users = new List<Nitter.User>();
                foreach (var Influencer in Slave.GetInfluencers())
                {
                    Users.Add(new Nitter.User(Influencer));
                }
                var Tweets = NitterSlave.UpdateTweets(Users, null, Slave.GetKeyWords());
                Tweets.Add("Manual",
                    new List<Nitter.Tweet>()
                    {
                    new Nitter.Tweet()
                    {
                        Link = link
                    }
                    });

                Slave.WriteTweets(Tweets);
            }
            else 
            {
                await botClient.SendTextMessageAsync(
                    chatId: ChatId,
                    text: $"Please provide correct twitter post link",
                    cancellationToken: cancellationToken);
            }
        }

        public async Task UpdatePostHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Type != UpdateType.Message)
                return;
            // Only process text messages
            if (update.Message!.Type != MessageType.Text)
                return;

            ChatId = update.Message.Chat.Id;

            await botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: "Starting updating twitts",
                cancellationToken: cancellationToken);


            var Users = new List<Nitter.User>();
            foreach (var Influencer in Slave.GetInfluencers())
            {
                Users.Add(new Nitter.User(Influencer));
            }

            var Tweets = NitterSlave.UpdateTweets(Users, null, Slave.GetKeyWords());
            Slave.WriteTweets(Tweets);

            await botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: "Done",
                cancellationToken: cancellationToken);
        }

        public async Task WatchHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var Man = update.Message.Text.Split(' ')[1].Replace("https://twitter.com/", "");
            var LastTweet = new TwitterScraper.Nitter.User(Man).GetTweets(1).First();
            bool IsSucceful;

            do 
            {
                IsSucceful = CheckingList.TryAdd(Man, LastTweet.Link);
            }
            while (!IsSucceful);

            await botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: "Succefully added to watchlist",
                cancellationToken: cancellationToken);
        }

        public async Task WriteTwittsHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Start Scrapping",
                cancellationToken: cancellationToken);

            List<Nitter.User> users = new List<Nitter.User>();
            var i = NitterAPI.NitterSlave.UpdateComments(DateTime.Today);
            Slave.WriteComments(i);

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Done",
                cancellationToken: cancellationToken);
        }

        public async Task HandleSearchingAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Start Searching",
                cancellationToken: cancellationToken);

            // Keywords for searching in Twitter
            var Keywordrs = Slave.GetSearching();
            // Store for data who will go to google sheets
            var result = new List<string>();

            Keywordrs.ForEach(keyword =>
            {
                foreach (var Result in NitterAPI.NitterSlave.SearchToday(keyword, 10))
                {
                    if (Result != null) result.Add(Result.Link);
                    else break;
                }
            });
            Slave.WriteSearchingResults(result);

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Done",
                cancellationToken: cancellationToken);
        }
        private static async void CheckAccountAsync(ITelegramBotClient botClient) 
        {
            while (true)
            {
                foreach (var watcher in CheckingList.Keys)
                {
                    /* If the last tweet from the user is not the same one that has been stored since it was added to the observation, 
                     * then notify it in the chat and save the new tweet */
                    var LastPost = new Nitter.User(watcher).GetLastTweet().Link;
                    if (LastPost != CheckingList[watcher])
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: ChatId,
                            text: $"{watcher} just posted new tweet {LastPost}",
                            cancellationToken: new CancellationToken());
                        CheckingList[watcher] = LastPost;
                    }
                }
                Thread.Sleep(100000);
            }
        }
    }
}
