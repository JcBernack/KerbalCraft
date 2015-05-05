using System;
using System.Text;

namespace CraftShare
{
    public static class StringCompressor
    {
        public static string Compress(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            bytes = CLZF2.Compress(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static string Decompress(string input)
        {
            var bytes = Convert.FromBase64String(input);
            bytes = CLZF2.Decompress(bytes);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}