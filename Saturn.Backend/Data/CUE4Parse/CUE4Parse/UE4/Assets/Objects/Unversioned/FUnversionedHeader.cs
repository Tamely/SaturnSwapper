using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CUE4Parse.UE4.Readers;
using CUE4Parse.Utils;
using Saturn.Backend.Data;

namespace CUE4Parse.UE4.Assets.Objects.Unversioned
{
    public class FUnversionedHeader
    {
        public static List<List<FFragment>> FFragments = new();
        public static List<BitArray> ZeroMasks = new();

        public IReadOnlyList<FFragment> Fragments;
        public BitArray ZeroMask;
        public readonly bool HasNonZeroValues;
        public bool HasValues => HasNonZeroValues | ZeroMask.Length > 0;
        
        public FUnversionedHeader(FArchive Ar)
        {
            var fragments = new List<FFragment>();
            
            FFragment fragment;
            var zeroMaskNum = 0;
            uint unmaskedNum = 0;
            
            do
            {
                ushort t = Ar.Read<ushort>();
                fragment = new FFragment(t);
                fragments.Add(fragment);
                
                if (fragment.HasAnyZeroes)
                    zeroMaskNum += fragment.ValueNum;
                else
                    unmaskedNum += fragment.ValueNum;
            } while (!fragment.IsLast);

            if (zeroMaskNum > 0)
            {
                LoadZeroMaskData(Ar, zeroMaskNum, out ZeroMask);
                HasNonZeroValues = unmaskedNum > 0 || ZeroMask.Contains(false);
            }
            else
            {
                ZeroMask = new BitArray(0);
                HasNonZeroValues = unmaskedNum > 0;
            }
            Fragments = fragments.ToList();

            if (IoPackage.NameToBeSearching.All(x => x.ToLower() != Ar.Name.SubstringAfterLast("/").ToLower().Split('.')[0])) return;
            
            Logger.Log("Adding fragments to " + Ar.Name.SubstringAfterLast("/").Split('.')[0]);
            FFragments.Add(fragments);
            ZeroMasks.Add(ZeroMask);
        }
        
        private static void LoadZeroMaskData(FArchive reader, int numBits, out BitArray data)
        {
            if (numBits <= 8)
            {
                data = new BitArray(new[] { reader.Read<byte>() });
            }
            else if (numBits <= 16)
            {
                data = new BitArray(new []{ (int) reader.Read<ushort>() });
            }
            else
            {
                var num = numBits.DivideAndRoundUp(32);
                var intData = new int[num];
                for (int idx = 0; idx < num; idx++)
                {
                    intData[idx] = reader.Read<int>();
                }
                data = new BitArray(intData);
            }

            data.Length = numBits;
        }
    }
}