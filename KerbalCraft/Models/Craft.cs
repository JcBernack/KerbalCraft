using System;

namespace KerbalCraft.Models
{
    public class Craft
    {
        public string _id { get; set; }
        public DateTime date { get; set; }
        public CraftUser author { get; set; }
        public CraftInfo info { get; set; }
        public int downloads { get; set; }

        public byte[] CraftCache;
        public byte[] ThumbnailCache;
    }
}