using System;

namespace CraftShare
{
    public class SharedCraft
    {
        public string _id { get; set; }
        public DateTime date { get; set; }
        public string author { get; set; }
        public CraftInfo info { get; set; }

        public byte[] CraftCache;
        public byte[] ThumbnailCache;
    }

    public class CraftInfo
    {
        public string ship { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public string partCount { get; set; }
        public string size { get; set; }
    }
}