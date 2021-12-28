using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Utils
{
    public class AnyLength
    {
        private static readonly byte[] FileEnd = { 248, 112 };
        private static readonly byte[] DefaultGameData = { 4, 8, 118, 255 };

        public static void ReplaceAnyLength(ref byte[] data, byte[] search, byte[] replace)
        {
            var bytes = new List<byte>(data);

            // Get our search offset
            var searchOffset = IndexOfSequence(data, search);

            // Delete our search string and insert our new replace string
            bytes.RemoveRange(searchOffset, search.Length);
            bytes.InsertRange(searchOffset, replace);

            // Change the 1 byte int of the search length
            bytes[searchOffset - 1] = Convert.ToByte(replace.Length);

            // Remove the difference in length
            if (FindByteArray(FileEnd, bytes.ToArray()))
                bytes.RemoveRange(IndexOfSequence(bytes.ToArray(), FileEnd) - 245, replace.Length - search.Length);
            else
                bytes.RemoveRange(IndexOfSequence(bytes.ToArray(), DefaultGameData) - 245, replace.Length - search.Length);

            data = bytes.ToArray();
        }

        // Originally: https://stackoverflow.com/a/332667/12897035
        private static int IndexOfSequence(byte[] buffer, byte[] pattern)
        {
            var i = Array.IndexOf(buffer, pattern[0], 0);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                var segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    return i;
                i = Array.IndexOf(buffer, pattern[0], i + 1);
            }

            return -1;
        }

        // Find a byte[] in a byte[] and return true if found
        private static bool FindByteArray(byte[] search, byte[] data)
        {
            var searchOffset = IndexOfSequence(data, search);
            return searchOffset != -1;
        }
    }
}
