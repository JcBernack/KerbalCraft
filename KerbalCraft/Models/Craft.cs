using System;
using System.Collections.Generic;
using System.Linq;

namespace KerbalCraft.Models
{
    public class Craft
    {
        public string _id { get; set; }
        public DateTime date { get; set; }
        public CraftUser author { get; set; }
        public Dictionary<string, string> info { get; set; }
        public int downloads { get; set; }

        public byte[] CraftCache;
        public byte[] ThumbnailCache;

        public const string DescriptionKey = "Description";

        public string Description
        {
            get { return info[DescriptionKey]; }
        }

        public static IEnumerable<string> GetInfoNames()
        {
            yield return "Name";
            yield return "Type";
            yield return "Author";
            yield return "Downloads";
        }

        public IEnumerable<string> GetInfoValues()
        {
            yield return info["Name"];
            yield return info["Type"];
            yield return author.username;
            yield return downloads.ToString();
        }

        public IEnumerable<string> GetExtendedInfoNames()
        {
            foreach (var pair in info.Where(_ => _.Key != DescriptionKey))
            {
                yield return pair.Key;
            }
            yield return "Author";
            yield return "Date";
            yield return "Downloads";
        }

        public IEnumerable<string> GetExtendedInfoValues()
        {
            foreach (var pair in info.Where(_ => _.Key != DescriptionKey))
            {
                yield return pair.Value;
            }
            yield return author.username;
            yield return date.ToLongDateString();
            yield return downloads.ToString();
        }
    }
}