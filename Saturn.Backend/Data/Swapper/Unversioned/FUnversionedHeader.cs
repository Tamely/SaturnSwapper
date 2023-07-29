using System;
using System.Collections;
using System.Collections.Generic;
using GenericReader;

namespace Saturn.Backend.Data.Swapper.Unversioned;

// https://github.com/EpicGames/UnrealEngine/blob/master/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L414

/// <summary>
/// List of serialized property indices and which of them are non-zero.
/// Serialized as a stream of 16-bit skip-x keep-y fragments and a zero bitmask.
/// </summary>
public class FUnversionedHeader
{
    public LinkedList<FFragment> Fragments;
    public LinkedListNode<FFragment> CurrentFragment;
    public int UnversionedPropertyIndex = 0;
    public int ZeroMaskIndex = 0;
    public uint ZeroMaskNum = 0;
    public BitArray ZeroMask;
    public bool bHasNonZeroValues = false;

    public void Read(GenericBufferReader reader)
    {
        Fragments = new LinkedList<FFragment>();

        FFragment Fragment;
        uint UnmaskedNum = 0;
        int firstNum = 0;
        do
        {
            Fragment = FFragment.Unpack(reader.Read<ushort>());
            Fragment.FirstNum = firstNum + Fragment.SkipNum;
            firstNum = firstNum + Fragment.SkipNum + Fragment.ValueNum;
            Fragments.AddLast(Fragment);

            if (Fragment.bHasAnyZeroes)
            {
                ZeroMaskNum += (uint)Fragment.ValueNum;
            }
            else
            {
                UnmaskedNum += (uint)Fragment.ValueNum;
            }
        } while (!Fragment.bIsLast);

        if (ZeroMaskNum > 0)
        {
            LoadZeroMaskData(reader, ZeroMaskNum);
            bHasNonZeroValues = UnmaskedNum > 0 || !CheckIfZeroMaskDataIsAllOnes();
        }
        else
        {
            ZeroMask = new BitArray(0);
            bHasNonZeroValues = UnmaskedNum > 0;
        }

        CurrentFragment = Fragments.First;
        UnversionedPropertyIndex = CurrentFragment.Value.FirstNum;
    }

    public void LoadZeroMaskData(GenericBufferReader reader, uint NumBits)
    {
        if (NumBits <= 8)
        {
            ZeroMask = new BitArray(reader.ReadArray<byte>(1));
        }
        else if (NumBits <= 16)
        {
            ZeroMask = new BitArray(reader.ReadArray<byte>(2));
        }
        else
        {
            var num = (NumBits + 32 - 1) / 32;
            var intData = new int[num];
            for (int idx = 0; idx < num; idx++)
            {
                intData[idx] = reader.Read<int>();
            }
            ZeroMask = new BitArray(intData);
        }
    }

    public byte[] SaveZeroMaskData()
    {
        int NumBits = ZeroMask.Length;

        byte[] res;
        if (NumBits <= 8)
        {
            res = new byte[1];
        }
        else if (NumBits <= 16)
        {
            res = new byte[2];
        }
        else
        {
            res = new byte[(NumBits + 32 - 1) / 32 * 4];
        }

        ZeroMask.CopyTo(res, 0);
        return res;
    }

    public bool CheckIfZeroMaskDataIsAllOnes()
    {
        for (int i = 0; i < ZeroMask.Length; i++)
        {
            if (!ZeroMask[i]) return false;
        }

        return true;
    }

    public byte[] Serialize()
    {
        List<byte> Data = new();
        foreach (var fragment in Fragments)
        {
            Data.AddRange(BitConverter.GetBytes(fragment.Pack()));
        }

        if (ZeroMask.Length > 0)
        {
            Data.AddRange(SaveZeroMaskData());
        }

        return Data.ToArray();
    }

    public bool HasValues()
    {
        return bHasNonZeroValues | (ZeroMask.Length > 0);
    }

    public bool HasNonZeroBalues()
    {
        return bHasNonZeroValues;
    }

    public FUnversionedHeader(GenericBufferReader reader)
    {
        Read(reader);
    }

    public FUnversionedHeader()
    {
        
    }
}