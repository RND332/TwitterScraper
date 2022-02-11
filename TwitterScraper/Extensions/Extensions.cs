using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterScraper.Nitter;

namespace TwitterScraper.Extensions
{
    public static class Extensions
    {
        public static List<string> WithoutFirstItem(this IList<object> list) 
        {
            list.RemoveAt(0);
            var ResultList = new List<string>();
            foreach (var item in list) 
            {
                ResultList.Add((string)item);
            }
            return ResultList;
        }
        public static List<string> WithoutFirstItem(this List<string> list)
        {
            list.RemoveAt(0);
            var ResultList = new List<string>();
            foreach (var item in list)
            {
                ResultList.Add((string)item);
            }
            return ResultList;
        }
        public static List<string> GetLinks(this List<Tweet> list)
        {
            var ResultList = new List<string>();
            foreach (var item in list)
            {
                ResultList.Add(item.Link);
            }
            return ResultList;
        }
    }
}
