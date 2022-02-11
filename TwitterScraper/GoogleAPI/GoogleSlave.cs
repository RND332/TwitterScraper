using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterScraper.Person;
using TwitterScraper.Extensions;
using TwitterScraper.Nitter;

namespace TwitterScraper.GoogleAPI
{
    internal class TablesSlave
    {
        private GoogleClient client;
        public TablesSlave(GoogleClient client) 
        {
            this.client = client;
        }
        public List<string> GetInfluencers() 
        {
            List<string> Influencers = new List<string>();

            var Config = client.ReadSheet("Accounts", "A1", "Z100");

            for (int i = 0; i < Config.Values[0].Count; i++)
            {
                Influencers.Add(Config.Values[0][i].ToString());
            }
            return Influencers;
        }
        public List<string> GetKeyWords() 
        {
            List<string> KeyWords = new List<string>();

            var Config = client.ReadSheet("Accounts", "A1", "Z100");

            for (int i = 0; i < Config.Values[1].Count; i++)
            {
                KeyWords.Add(Config.Values[1][i].ToString());
            }
            return KeyWords;
        }
        public List<Person.Person> GetPersons() 
        {
            List<Person.Person> people = new List<Person.Person>();

            var Config = client.ReadSheet("Comments", "A1", "Z100");

            for (int i = 0; i < Config.Values.Count; i++) 
            {
                people.Add(
                    new Person.Person(
                        Config.Values[i][0].ToString(),
                        Config.Values[i].WithoutFirstItem()));
            }
            return people;
        }
        public List<string> GetSearching()
        {
            var people = new List<string>();

            var Config = client.ReadSheet("Comments", "C1", "C100");

            for (int i = 0; i < Config.Values[0].Count; i++)
            {
                people.Add(Config.Values[0][i].ToString());
            }
            return people.WithoutFirstItem();
        }
        public void WriteComments(List<Person.Person> persons) 
        {
            List<List<string>> Comments = new List<List<string>>();
            for (int i = 0; i < persons.Count; i++)
            {
                if (persons[i] == null) continue;
                Comments.Add(new List<string>() { persons[i].Name });
                foreach (var comment in persons[i].Comments)
                {
                    Comments.Add(new List<string> { comment.Post, comment.Reply, comment.Text });
                }
            }
            client.WriteSheet(Comments, "ROWS", StartCell: "A1", EndCell: "Z100");
        }
        public void WriteTweets(Dictionary<string, List<Tweet>> DictTweet) 
        {
            List<List<string>> Comments = new List<List<string>>();
            foreach(var key in DictTweet.Keys)
            {
                if (DictTweet[key].Count == 0)
                {
                    DictTweet.Remove(key);
                }
            }

            foreach (var key in DictTweet.Keys)
            {
                List<string> RND = new List<string>();
                RND.Add(key);
                foreach (var tweet in DictTweet[key])
                {
                    RND.Add(tweet.Link);
                }
                Comments.Add( RND );
            }
            client.WriteSheet(Comments, "COLUMNS", TableName: "Posts", StartCell: "A", EndCell: "Z");
        }
        public void WriteSearchingResults(List<string> vs) 
        {
            List<List<string>> SearchingresultsToWrite = new List<List<string>>();
            SearchingresultsToWrite.Add(vs);

            if (SearchingresultsToWrite != null)
            {
                client.WriteSheet(SearchingresultsToWrite, "COLUMNS", "Searching");
            }
        }
    }

}
