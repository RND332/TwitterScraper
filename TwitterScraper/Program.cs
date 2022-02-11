using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TwitterScraper.GoogleAPI;
using TwitterScraper.Person;
using TwitterScraper.Nitter;
using TwitterScraper.Extensions;
using TwitterScraper.NitterAPI;
using System.Diagnostics;
using TwitterScraper.Telegram;

namespace SheetsQuickstart
{
    class Program 
    {
        static void Main(string[] args)
        {
            new NitterWorker().DockerCompose();

            TelegramSlave telegram = new TelegramSlave();
            Console.ReadKey();
        }
    }
}
