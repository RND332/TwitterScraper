using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitterScraper.GoogleAPI;
using TwitterScraper.Nitter;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace TwitterScraper.NitterAPI
{
    class NitterSlave
    {
        static TablesSlave Slave = new TablesSlave(new GoogleClient());
        public static List<Tweet> UpdateTweets(List<User> users, DateTime? offset) 
        {
            var tweets = new List<Tweet>();
            var Tasks = new List<Thread>();

            foreach (var user in users)
            {
                Tasks.Add(new Thread(() =>
                {
                    if (offset == null) offset = new DateTime(2022, 02, 05);
                    foreach (var tweet in user.GetTweets())
                    {
                        if (tweet.DateTime.CompareTo(offset) == -1)
                        {
                            break;
                        }
                        if (!(tweet.IsReply || tweet.IsRetweet))
                        {
                            tweets.Add(tweet);
                        }
                    }
                }));
            }
            foreach (var Task in Tasks) Task.Start();
            foreach (var Task in Tasks) Task.Join();

            return tweets;
        }
        public static Dictionary<string, List<Tweet>> UpdateTweets(List<User> users, DateTime? offset, List<string> keywords) 
        {
            Dictionary<string, List<Tweet>> pairs = new Dictionary<string, List<Tweet>>();
            keywords.ForEach(key => pairs.Add(key, new List<Tweet>()));
            var ALL = new List<Tweet>();
            var Tasks = new List<Thread>();

            foreach (var user in users)
            {
                Tasks.Add(new Thread(() =>
                {
                    if (offset == null) offset = DateTime.Today;
                    foreach (var tweet in user.GetTweets())
                    {
                        if (tweet.DateTime.CompareTo(offset) == -1)
                        {
                            break;
                        }
                        if (!(tweet.IsReply || tweet.IsRetweet))
                        {
                            ALL.Add(tweet);
                            foreach (var key in keywords) 
                            {
                                if (key.Contains(",")) 
                                {
                                    List<bool> Checks = new List<bool>();
                                    foreach (var MultpleKey in key.Split(",")) 
                                    {
                                        Checks.Add(tweet.Text.Contains(MultpleKey));
                                    }

                                    if (Checks.All(Check => Check)) 
                                    {
                                        pairs[key].Add(tweet);
                                    }
                                }
                                if (tweet.Text.Contains(key)) 
                                {
                                    pairs[key].Add(tweet);
                                }
                            }
                        }
                    }
                }));
            }
            foreach (var Task in Tasks) Task.Start();
            foreach (var Task in Tasks) Task.Join();
            pairs.Add("All", ALL);

            return pairs;
        }
        public static List<Person.Person> UpdateComments(DateTime? offset) 
        {
            var tweets = new List<Tweet>();
            var Tasks = new List<Thread>();
            var people = Slave.GetPersons();
            var Result = new List<Person.Person>();

            foreach (var user in people)
            {
                Tasks.Add(new Thread(() =>
                {
                    Person.Person person = null;
                    if (offset == null) offset = DateTime.Today;
                    foreach (var TwitterAccount in user.Accounts)
                    {
                        person = new Person.Person(user.Name, new List<string>(user.Accounts));
                        foreach (var tweet in new Nitter.User(TwitterAccount).GetReplyes())
                        {
                            if (tweet.IsPin) 
                            {
                                continue;
                            }
                            if (tweet.DateTime.CompareTo(offset) == -1)
                            {
                                break;
                            }
                            if (tweet.IsReply)
                            {
                                person.Comments.Add(new Person.Comment(tweet.Link, tweet.Link, tweet.Text));
                            }
                        }
                    }
                    Result.Add(person);
                }));
            }
            foreach (var Task in Tasks) Task.Start();
            foreach (var Task in Tasks) Task.Join();

            return Result;
        }
        public static IEnumerable<Tweet> SearchToday(string q, int limit) 
        {
            int count = 0;
            var Filters = $"http://localhost:8080" + $"/search?f=tweets&q={q}&e-nativeretweets=on&e-replies=on&since={DateTime.Today.ToString("yyyy-MM-dd")}";
            var HTML = new WebClient().DownloadString(Filters);
            var document = new HtmlParser().ParseDocument(HTML);
            while (true)
            {
                var tweetsvs = document.GetElementsByClassName("timeline-item");
                if (tweetsvs.Count() == 0) yield return null;
                foreach (var tweet in document.GetElementsByClassName("timeline-item"))
                {
                    var SearchedTweet = new Tweet();
                    SearchedTweet.Link = "https://twitter.com" + tweet.GetElementsByClassName("tweet-link").First().GetAttribute("href");
                    SearchedTweet.Text = tweet.GetElementsByClassName("tweet-content media-body").First().Text();

                    yield return SearchedTweet;
                    count++;
                }
                if (count == limit) break;
                var Next = document.GetElementsByClassName("show-more").First().GetAttribute("href");
                Filters = $"http://localhost:8080" + Next;
                HTML = new WebClient().DownloadString(Filters);
                document = new HtmlParser().ParseDocument(HTML);
            }
        }
    }
}
