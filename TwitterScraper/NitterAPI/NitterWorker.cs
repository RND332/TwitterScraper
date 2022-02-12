using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Docker.DotNet;

namespace TwitterScraper.Nitter
{
    internal class NitterWorker : NitterInterface
    {
        public static string NitterURL = "http://localhost:8080/";
        public void DockerCompose()
        {
            var cmd = System.Diagnostics.Process.Start("cmd.exe", "/C cd nitter && docker compose up -d");
            cmd.WaitForExit();

            Console.WriteLine("Done");
        }

    }
    class User
    {
        public string Username { get; set; }
        public string Link { get; set; }
        public User(string Username) 
        {
            this.Username = Username.Replace("https://twitter.com/", "");
            this.Link = "https://twitter.com/" + this.Username;
        }
        public IEnumerable<Tweet> GetTweets()
        {
            string URL = NitterWorker.NitterURL + this.Username;

            // Download User page HTML as string 
            var HTML = new WebClient().DownloadString(URL);
            // Parse HTML into document
            var document = new HtmlParser().ParseDocument(HTML);

            var RawTweet = document.GetElementsByClassName("timeline-item");

            for (int i = 0; i < RawTweet.Count(); i++)
            {
                var tweet = new Tweet();

                var element = RawTweet[i];
                tweet.Link = "https://twitter.com" + element.GetElementsByClassName("tweet-link").First()
                    .GetAttribute("href").ToString();
                tweet.Text = element.GetElementsByClassName("tweet-content media-body").First().TextContent;

                var stats = element.GetElementsByClassName("tweet-stat");
                tweet.Replyes = stats[0].TextContent.Replace(",", "") != "" ? int.Parse(stats[0].TextContent.Replace(",", "")) : 0;
                tweet.Retweets = stats[1].TextContent.Replace(",", "") != "" ? int.Parse(stats[1].TextContent.Replace(",", "")) : 0;
                tweet.Likes = stats[3].TextContent.Replace(",", "") != "" ? int.Parse(stats[3].TextContent.Replace(",", "")) : 0;

                tweet.DateTime = 
                    Tweet.ParseDateTime(element.GetElementsByClassName("tweet-date")
                    .Children("").First().GetAttribute("title"));

                var debug = element.GetElementsByClassName("retweet-header").Count();
                if (element.GetElementsByClassName("retweet-header").Count() > 0)
                {
                    tweet.IsRetweet = true;
                }
                if (element.GetElementsByClassName("replying-to").Count() > 0)
                {
                    tweet.IsReply = true;
                }

                yield return tweet;
                
                if (!(i < RawTweet.Count() - 1))
                {
                    var NextLink = document.GetElementsByClassName("show-more").Last().Children[0].GetAttribute("href").ToString();
                    HTML = new WebClient().DownloadString(URL + NextLink);
                    document = new HtmlParser().ParseDocument(HTML);
                    RawTweet = document.GetElementsByClassName("timeline-item");
                    i = 0;
                }
            }
        }
        public IEnumerable<Tweet> GetReplyes()
        {
            string URL = NitterWorker.NitterURL + this.Username + "/with_replies";
            string HTML;
            try
            {
                // Download User page HTML as string 
                HTML = new WebClient().DownloadString(URL);
            }
            catch (Exception ex) 
            {
                yield break;
            }
            // Parse HTML into document
            var document = new HtmlParser().ParseDocument(HTML);

            var RawTweet = document.GetElementsByClassName("timeline-item");

            for (int i = 0; i < RawTweet.Count(); i++)
            {
                var tweet = new Tweet();

                var element = RawTweet[i];
                tweet.Link = "https://twitter.com" + element.GetElementsByClassName("tweet-link").First()
                    .GetAttribute("href").ToString();
                tweet.Text = element.GetElementsByClassName("tweet-content media-body").First().TextContent;

                var stats = element.GetElementsByClassName("tweet-stat");
                tweet.Replyes = stats[0].TextContent.Replace(",", "") != "" ? int.Parse(stats[0].TextContent.Replace(",", "")) : 0;
                tweet.Retweets = stats[1].TextContent.Replace(",", "") != "" ? int.Parse(stats[1].TextContent.Replace(",", "")) : 0;
                tweet.Likes = stats[3].TextContent.Replace(",", "") != "" ? int.Parse(stats[3].TextContent.Replace(",", "")) : 0;

                tweet.DateTime =
                    Tweet.ParseDateTime(element.GetElementsByClassName("tweet-date")
                    .Children("").First().GetAttribute("title"));

                if (element.GetElementsByClassName("retweet-header").Count() > 0)
                {
                    tweet.IsRetweet = true;
                }
                if (element.GetElementsByClassName("replying-to").Count() > 0)
                {
                    tweet.IsReply = true;
                }
                if (element.GetElementsByClassName("pinned").Count() > 0)
                {
                    tweet.IsPin = true;
                }

                yield return tweet;

                // Get next page URL for catching more tweets
                // "If Theres no more tweets on this page, get link from "Show more" button and go to her"
                if (!(i < RawTweet.Count() - 1))
                {
                    var NextLink = document.GetElementsByClassName("show-more").Last().Children[0].GetAttribute("href").ToString();
                    HTML = new WebClient().DownloadString(URL + NextLink);
                    document = new HtmlParser().ParseDocument(HTML);
                    RawTweet = document.GetElementsByClassName("timeline-item");
                    i = 0;
                }
            }
        }
        public IEnumerable<Tweet> GetTweets(int limit)
        {
            int LimitChecker = 0;
            string URL = "";

            if (this.Username.Contains("/"))
            {
                URL = NitterWorker.NitterURL + this.Username.Remove(Username.IndexOf("/"));
            }
            else 
            {
                URL = NitterWorker.NitterURL + this.Username;
            }

            // Download User page HTML as string 
            var HTML = new WebClient().DownloadString(URL);
            // Parse HTML into document
            var document = new HtmlParser().ParseDocument(HTML);

            var RawTweet = document.GetElementsByClassName("timeline-item");

            for (int i = 0; i < RawTweet.Count(); i++)
            {
                var tweet = new Tweet();

                var element = RawTweet[i];
                tweet.Link = "https://twitter.com" + element.GetElementsByClassName("tweet-link").First()
                    .GetAttribute("href").ToString();
                tweet.Text = element.GetElementsByClassName("tweet-content media-body").First().TextContent;

                var stats = element.GetElementsByClassName("tweet-stat");
                tweet.Replyes = stats[0].TextContent.Replace(",", "") != "" ? int.Parse(stats[0].TextContent.Replace(",", "")) : 0;
                tweet.Retweets = stats[1].TextContent.Replace(",", "") != "" ? int.Parse(stats[1].TextContent.Replace(",", "")) : 0;
                tweet.Likes = stats[3].TextContent.Replace(",", "") != "" ? int.Parse(stats[3].TextContent.Replace(",", "")) : 0;

                tweet.DateTime =
                    Tweet.ParseDateTime(element.GetElementsByClassName("tweet-date")
                    .Children("").First().GetAttribute("title"));

                var debug = element.GetElementsByClassName("retweet-header").Count();
                if (element.GetElementsByClassName("retweet-header").Count() > 0)
                {
                    tweet.IsRetweet = true;
                }
                if (element.GetElementsByClassName("replying-to").Count() > 0)
                {
                    tweet.IsReply = true;
                }

                tweet.Username = this.Username;

                yield return tweet;
                LimitChecker += 1;
                if (LimitChecker == limit) break;

                if (!(i < RawTweet.Count() - 1))
                {
                    var NextLink = document.GetElementsByClassName("show-more").Last().Children[0].GetAttribute("href").ToString();
                    HTML = new WebClient().DownloadString(URL + NextLink);
                    document = new HtmlParser().ParseDocument(HTML);
                    RawTweet = document.GetElementsByClassName("timeline-item");
                    i = 0;
                }
            }
        }
        public Tweet GetLastTweet()
        {
            var LastOne = GetTweets(1);
            return LastOne.First();
        }
    }

    public class Tweet
    {
        public string Username { get; set; }
        public string Link { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsRetweet { get; set; }
        public bool IsReply { get; set; }
        public bool IsPin { get; set; }
        public int Likes { get; set; }
        public int Retweets { get; set; }
        public int Replyes { get; set; }
        public string Text { get; set; }
        public static DateTime ParseDateTime(string datetime) 
        {
            // Feb 1, 2022 · 12:44 PM UTC

            var all = datetime.Remove(datetime.IndexOf(" UTC")).Split(" · ");

            var Date = DateTime.Parse(all[0]).Date;
            var Time = DateTime.Parse(all[1], CultureInfo.InvariantCulture);

            DateTime result = new DateTime();
            result = result.AddYears(Date.Year - 1);
            result = result.AddMonths(Date.Month - 1);
            result = result.AddDays(Date.Day - 1);
            result = result.AddHours(Time.Hour - 1);
            result = result.AddMinutes(Time.Minute - 1);
            return result;
        }
    }
}
