using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Saturn.Backend.Data.Enums;

namespace Saturn.Backend.Data.Utils
{
    public class AnyLength
    {
        private static readonly byte[] End = { 248, 112 };

        public static bool TrySwap(ref byte[] array, List<byte[]> searches, List<byte[]> replaces)
        {
            try
            {
                var arr = new List<byte>(array);
                int diff = 0;

                if (searches.Count != replaces.Count)
                    throw new ArgumentException("Searches' size does not equal replaces' size!");

                int lastOffset = IndexOfSequence(array, Encoding.ASCII.GetBytes("/Game/")) - 1;
                int startOffset = lastOffset - array[44] * 2 + 2;

                for (int i = 0; i < searches.Count; i++)
                {
                    int searchOffset = IndexOfSequence(arr.ToArray(), searches[i]);
                    int sizeOffset = NumFromTop(arr.ToArray(), startOffset, lastOffset, searchOffset, array[44]);
                    arr.RemoveAt(sizeOffset);
                    arr.Insert(sizeOffset, (byte)replaces[i].Length);
                
                    arr.RemoveRange(searchOffset, searches[i].Length);
                    arr.InsertRange(searchOffset, replaces[i]);
                    diff += searches[i].Length - replaces[i].Length;

                }

                int end = IndexOfSequence(arr.ToArray(), End) - 3;
                int a = 0;
                List<byte> append = new();
                if (diff > -1)
                {
                    while (diff > a)
                    {
                        append.Add(0);
                        a++;
                    }

                    arr.InsertRange(end, append);
                }
                else
                {
                    while (array.Length != arr.Count)
                    {
                        int newEnd = end + diff;
                        arr.RemoveRange(newEnd, Math.Abs(diff));
                    }
                }

                array = arr.ToArray();
                return true;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, LogLevel.Error);
                throw new Exception("Failed to swap!");
            }
        }

        //Originally: https://stackoverflow.com/a/332667/12897035
        private static int IndexOfSequence(byte[] buffer, byte[] pattern)
        {
            int i = Array.IndexOf(buffer, pattern[0], 0);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    return i;
                i = Array.IndexOf(buffer, pattern[0], i + 1);
            }

            return -1;
        }

        private static int NumFromTop(byte[] arr, int first, int last, int currentOffset, int numExports)
        {
            for (int i = 0; i < numExports; i++)
            {
                if (last + 1 != currentOffset)
                {
                    last = last + arr[first + i * 2];
                }
                else
                {
                    return first + i * 2;
                }
            }

            return -1;
        }
    }
}