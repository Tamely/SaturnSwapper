using System;

namespace Saturn.Backend.Data.Swapper.Unversioned;

/// <summary>
/// Unversioned header fragment.
/// </summary>
public class FFragment
{
    /// <summary>
    /// Number of properties to skip before values.
    /// </summary>
    public int SkipNum;
    
    /// <summary>
    /// Number of following property values stored.
    /// </summary>
    public int ValueNum = 0;
    
    /// <summary>
    /// Is this the last fragment in the header?
    /// </summary>
    public bool bIsLast = false;

    public int FirstNum = -1;
    public int LastNum
    {
        get
        {
            return FirstNum + ValueNum - 1;
        }
    }

    public bool bHasAnyZeroes = false;

    internal static readonly byte SkipMax = 127;
    internal static readonly byte ValueMax = 127;
    internal static readonly uint SkipNumMask = 0x007fu;
    internal static readonly uint HasZeroMask = 0x0080u;
    internal static readonly int ValueNumShift = 9;
    internal static readonly uint IsLastMask = 0x0100u;

    public override string ToString()
    {
        return "{" + SkipNum + "," + ValueNum + "," + bHasAnyZeroes + "," + bIsLast + "}";
    }
    
    public ushort Pack()
    {
        if (SkipNum > SkipMax) throw new InvalidOperationException("Skip num " + SkipNum + " is greater than maximum possible value " + SkipMax);
        if (ValueNum > ValueMax) throw new InvalidOperationException("Value num " + ValueNum + " is greater than maximum possible value " + ValueMax);
        return (ushort)((byte)SkipNum | (bHasAnyZeroes ? HasZeroMask : 0) | (ushort)((byte)ValueNum << ValueNumShift) | (bIsLast ? IsLastMask : 0));
    }

    public static FFragment Unpack(ushort Int)
    {
        return new FFragment
        {
            SkipNum = (byte)(Int & SkipNumMask),
            bHasAnyZeroes = (Int & HasZeroMask) != 0,
            ValueNum = (byte)(Int >> ValueNumShift),
            bIsLast = (Int & IsLastMask) != 0
        };
    }

    public static FFragment GetFromBounds(int LastNumBefore, int FirstNum, int LastNum, bool hasAnyZeros, bool isLast) // for 1st fragment: LastNumBefore = -1
    {
        return new FFragment
        {
            SkipNum = FirstNum - LastNumBefore - 1,
            ValueNum = LastNum - FirstNum + 1,
            bHasAnyZeroes = hasAnyZeros,
            bIsLast = isLast,
            FirstNum = FirstNum
        };
    }

    public FFragment()
    {

    }

    public FFragment(int skipNum, int valueNum, bool bIsLast, bool bHasAnyZeroes)
    {
        SkipNum = skipNum;
        ValueNum = valueNum;
        this.bIsLast = bIsLast;
        this.bHasAnyZeroes = bHasAnyZeroes;
    }
}