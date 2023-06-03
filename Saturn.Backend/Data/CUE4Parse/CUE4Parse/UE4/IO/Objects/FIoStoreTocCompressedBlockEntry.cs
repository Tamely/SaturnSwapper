using CUE4Parse.UE4.Readers;

namespace CUE4Parse.UE4.IO.Objects
{
    public struct FIoStoreTocCompressedBlockEntry
    {
        private const int OffsetBits = 40;
        private const ulong OffsetMask = (1ul << OffsetBits) - 1ul;
        private const int SizeBits = 24;
        private const uint SizeMask = (1 << SizeBits) - 1;
        private const int SizeShift = 8;

        public long Position;
        public long Offset;
        public uint CompressedSize;
        public uint UncompressedSize;
        public byte CompressionMethodIndex;

        public FIoStoreTocCompressedBlockEntry(FArchive Ar)
        {
            Position = Ar.Position;
            unsafe
            {
                var data = stackalloc byte[5 + 3 + 3 + 1];
                Ar.Serialize(data, 5 + 3 + 3 + 1);
                Offset = (long) (*(ulong*) data & OffsetMask);
                CompressedSize = (*((uint*) data + 1) >> SizeShift) & SizeMask;
                UncompressedSize = *((uint*) data + 2) & SizeMask;
                CompressionMethodIndex = (byte) (*((uint*) data + 2) >> SizeBits);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Offset)} {Offset}: From {CompressedSize} To {UncompressedSize}";
        }
    }
}