using System;
using System.Text;

namespace CraftShare
{
    public static class StringCompressor
    {
        public static string Compress(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return BinaryCompressor.Compress(bytes);
        }

        public static string Decompress(string input)
        {
            var bytes = BinaryCompressor.Decompress(input);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    public static class BinaryCompressor
    {
        public static string Compress(byte[] input)
        {
            input = CLZF2.Compress(input);
            return Convert.ToBase64String(input);
        }

        public static byte[] Decompress(string input)
        {
            var bytes = Convert.FromBase64String(input);
            return CLZF2.Decompress(bytes);
        }
    }
}