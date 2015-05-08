using System;

namespace CraftShare
{
    public class SharedCraft
    {
        public string _id { get; set; }
        public DateTime date { get; set; }
        public string name { get; set; }
        public string facility { get; set; }
        public string author { get; set; }

        public byte[] CraftCache;
        public byte[] ThumbnailCache;
    }
}