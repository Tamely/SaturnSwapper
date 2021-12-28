using Saturn.Data.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Utils;

public class Engine
{
    public static bool Find(long start, byte[] bytes, string convert, long max = 0L)
    {
        var a = Encoding.UTF8.GetBytes(convert);
        var result = false;
        Stream s = new MemoryStream(bytes);
        var task = Task.Run(() => D(s, start + 1, a, max));
        var flag3 = task.Wait(TimeSpan.FromSeconds(10.0));
        long num;
        if (flag3)
        {
            num = task.Result;
            Vars.CurrentOffset = num;
            result = true;
        }
        else
        {
            num = 0L;
        }

        s.Close();
        var flag4 = num == 0L;
        if (flag4) result = false;

        return result;
    }

    public static bool FindHex(long start, byte[] bytes, byte[] a, long max = 0L)
    {
        var result = false;
        Stream s = new MemoryStream(bytes);
        var task = Task.Run(() => D(s, start + 1, a, max));
        var flag3 = task.Wait(TimeSpan.FromSeconds(10.0));
        long num;
        if (flag3)
        {
            num = task.Result;
            Vars.HexOffset = num;
            result = true;
        }
        else
        {
            num = 0L;
        }

        s.Close();
        var flag4 = num == 0L;
        if (flag4) result = false;

        return result;
    }

    public static bool FindStop(byte[] bytes, long max = 0L)
    {
        var a = new byte[] { 0 };
        var result = false;
        Stream s = new MemoryStream(bytes);
        var task = Task.Run(() => D(s, Vars.CurrentOffset, a, max));
        var flag3 = task.Wait(TimeSpan.FromSeconds(10.0));
        long num;
        if (flag3)
        {
            num = task.Result;
            Vars.StopOffset = num;
            result = true;
        }
        else
        {
            num = 0L;
        }

        s.Close();
        var flag4 = num == 0L;
        if (flag4) result = false;

        return result;
    }

    private static byte[] C(byte[] mahOldByteArray, byte newByte)
    {
        var list = new List<byte>();
        list.AddRange(mahOldByteArray);
        list.Add(newByte);
        return list.ToArray();
    }

    private static long D(Stream a, long b, byte[] c, long max)
    {
        var num = 0;
        var result = 0L;
        a.Position = b;
        var flag = false;
        var flag2 = max == 0L;
        if (flag2)
        {
            flag = false;
        }
        else
        {
            var flag3 = max > 1L;
            if (flag3) flag = true;
        }

        for (;;)
        {
            var flag4 = flag;
            if (flag4)
            {
                var flag5 = a.Position == max;
                if (flag5) break;
            }
            else
            {
                var flag6 = a.Position == 5000000000L;
                if (flag6) goto Block_5;
            }

            var num2 = a.ReadByte();
            var flag7 = num2 == -1;
            if (flag7) goto Block_6;
            var flag8 = num2 == c[num];
            if (flag8)
            {
                num++;
                var flag9 = num == c.Length;
                if (flag9) goto Block_8;
            }
            else
            {
                num = 0;
            }
        }

        return result;
        Block_5:
        return result;
        Block_6:
        return result;
        Block_8:
        return a.Position - c.Length;
    }
}