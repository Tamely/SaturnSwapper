﻿using Saturn.Backend.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saturn.Backend.Data.Utils
{
    public class AnyLength
    {
        private static readonly byte[] End = { 248, 112 };

        public static bool TrySwap(ref byte[] array, List<byte[]> searches, List<byte[]> replaces, bool isPickaxe = false)
        {
            try
            {
                if (!isPickaxe)
                {
                    var index = IndexOfSequence(array, End);
                    if (index == -1)
                    {
                        Logger.Log("Moving on to fallback swapping!");
                        if (!SwapNormally(searches, replaces, ref array))
                            throw new Exception("Failed to swap normally!");
                    }
                }

                var arr = new List<byte>(array);
                var diff = 0;

                if (searches.Count != replaces.Count)
                    throw new ArgumentException("Searches' size does not equal replaces' size!");

                var lastOffset = IndexOfSequence(array, Encoding.ASCII.GetBytes("/Game/")) - 1;
                var startOffset = lastOffset - array[44] * 2 + 2;

                for (var i = 0; i < searches.Count; i++)
                {
                    var searchOffset = IndexOfSequence(arr.ToArray(), searches[i]);
                    if (searches[i].Length != replaces[i].Length)
                    {
                        var sizeOffset = NumFromTop(arr.ToArray(), startOffset, lastOffset, searchOffset, array[44]);
                        if (sizeOffset < 0)
                            continue;
                        arr.RemoveAt(sizeOffset);
                        arr.Insert(sizeOffset, (byte)replaces[i].Length);
                    }
                    arr.RemoveRange(searchOffset, searches[i].Length);
                    arr.InsertRange(searchOffset, replaces[i]);
                    diff += searches[i].Length - replaces[i].Length;
                }


                var end = 0;
                if (isPickaxe)
                {
                    end = lastOffset + 1;
                    // foreach in range step 2
                    for (var i = startOffset; i <= lastOffset; i += 2)
                        end += arr[i];
                }
                else
                {
                    end = IndexOfSequence(arr.ToArray(), End) - 3;
                }

                var a = 0;
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
                        var newEnd = end + diff;
                        arr.RemoveRange(newEnd, Math.Abs(diff));
                    }
                }

                array = arr.ToArray();
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("You might be able to ignore this!!!!     " + e.ToString(), LogLevel.Error);
                Logger.Log("Moving on to fallback swapping!");
                if (!SwapNormally(searches, replaces, ref array))
                {
                    throw new Exception("Failed to swap!");
                }
                return true;
            }
        }

        public static bool SwapNormally(List<byte[]> Searches, List<byte[]> Replaces, ref byte[] array)
        {
            try
            {
                var arr = new List<byte>(array);
                for (var i = 0; i < Searches.Count; i++)
                {
                    var search = Searches[i];
                    var replace = Replaces[i];
                    var index = IndexOfSequence(array, search);
                    if (index == -1)
                    {
                        Logger.Log("Couldn't find search at index " + i + "!", LogLevel.Error);
                        continue;
                    }

                    if (replace.Length < search.Length)
                        Array.Resize(ref replace, search.Length);
                    else if (replace.Length > search.Length)
                        continue;

                    arr.RemoveRange(index, search.Length);
                    arr.InsertRange(index, replace);
                }
                array = arr.ToArray();
            }
            catch
            {
                return false;
            }

            return true;
        }

        //Originally: https://stackoverflow.com/a/332667/12897035
        public static int IndexOfSequence(byte[] buffer, byte[] pattern)
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

        private static int NumFromTop(byte[] arr, int first, int last, int currentOffset, int numExports)
        {
            for (var i = 0; i < numExports; i++)
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