using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterScraper.Person
{
    internal class Influencers 
    {
        public string Link { get; set; }
        public Influencers(string Link) 
        {
            this.Link = Link;
        }
    }
    internal class Person
    {
        /// <summary>
        /// RND, PAUL, etc...
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Twitter Accounts
        /// </summary>
        public List<string> Accounts { get; set; }
        /// <summary>
        /// Original post link -- reply link
        /// </summary>
        public List<Comment> Comments { get; set; }

        public Person(string Name, List<string> Accounts) 
        {
            this.Name = Name;
            this.Accounts = Accounts;
            this.Comments = new List<Comment>();
        }
        public void AddNewReply(string Post, string Reply, string Text) => this.Comments.Add(new Comment(Post, Reply, Text));
    }
    class Comment
    {
        public string Post { get; set; }
        public string Reply { get; set; }
        public string Text { get; set; }
        public Comment(string Post, string Reply, string Text)
        {
            this.Post = Post;
            this.Reply = Reply;
            this.Text = Text;
        }
    }
}
