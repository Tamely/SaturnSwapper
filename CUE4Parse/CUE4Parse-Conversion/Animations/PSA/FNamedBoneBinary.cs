﻿using CUE4Parse.UE4.Writers;
using CUE4Parse_Conversion.ActorX;

namespace CUE4Parse_Conversion.Animations.PSA
{
    /** Binary bone format to deal with raw animations as generated by various exporters. */
    public class FNamedBoneBinary
    {
        public const int SIZE = 64 + 3*4 + VJointPosPsk.SIZE;

        /** Bone's name */
        public string Name;
        /** reserved */
        public uint Flags;
        public int NumChildren;
        /** 0/NULL if this is the root bone. */
        public int ParentIndex;
        public VJointPosPsk BonePos;

        public void Serialize(FArchiveWriter Ar)
        {
            Ar.Write(Name, 64);
            Ar.Write(Flags);
            Ar.Write(NumChildren);
            Ar.Write(ParentIndex);
            BonePos.Serialize(Ar);
        }
    }
}