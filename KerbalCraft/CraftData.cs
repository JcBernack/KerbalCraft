using System;

namespace KerbalCraft
{
    public class CraftData
    {
        public string _id { get; set; }
        public DateTime date { get; set; }
        public string author { get; set; }
        public CraftInfo info { get; set; }

        public byte[] CraftCache;
        public byte[] ThumbnailCache;
    }
}