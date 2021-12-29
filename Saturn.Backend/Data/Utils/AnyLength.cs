using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Saturn.Backend.Data.Enums;

namespace Saturn.Backend.Data.Utils
{
    public class AnyLength
    {
        private static readonly byte[] End = { 248, 112 };

        public static bool TrySwap(ref byte[] array, List<byte[]> searches, List<byte[]> replaces, bool isRarity = false)
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
                    if (searches[i].Length < replaces[i].Length)
                        continue;
                    int searchOffset = IndexOfSequence(arr.ToArray(), searches[i]);
                    if (!isRarity)
                    {
                        int sizeOffset = NumFromTop(arr.ToArray(), startOffset, lastOffset, searchOffset, array[44]);
                        Logger.Log("Size offset is " + sizeOffset);
                        if (sizeOffset < 0)
                            continue;
                        arr.RemoveAt(sizeOffset);
                        arr.Insert(sizeOffset, (byte)replaces[i].Length);
                    }
                    arr.RemoveRange(searchOffset, searches[i].Length);
                    arr.InsertRange(searchOffset, replaces[i]);
                    diff += searches[i].Length - replaces[i].Length;

                }

                int end = lastOffset + 1;
                
                // foreach in range step 2
                for (int i = startOffset; i <= lastOffset; i += 2)
                    end += (int)arr[i];

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
                        Logger.Log($"Changing end from {end} to {end + diff}");
                        int newEnd = end + diff;
                        arr.RemoveRange(newEnd, Math.Abs(diff));
                    }
                }

                array = arr.ToArray();
                return true;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString(), LogLevel.Error);
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
        
        // Add 0 to the end of a byte[] to make it the same size as another array
        public static byte[] AddZero(byte[] array, int size)
        {
            var newArray = new byte[size];
            Array.Copy(array, newArray, array.Length);
            return newArray;
        }
    }
}