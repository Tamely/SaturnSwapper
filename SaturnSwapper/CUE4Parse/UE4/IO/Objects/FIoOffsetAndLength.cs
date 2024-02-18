using CUE4Parse.UE4.Readers;

namespace CUE4Parse.UE4.IO.Objects
{
    public struct FIoOffsetAndLength
    {
        public long Position;
        public ulong Offset;
        public ulong Length;

        public FIoOffsetAndLength(FArchive Ar)
        {
            Position = Ar.Position;
            unsafe
            {
                var offsetAndLength = stackalloc byte[10];
                Ar.Serialize(offsetAndLength, 10);
                Offset = offsetAndLength[4]
                         | ((ulong) offsetAndLength[3] << 8)
                         | ((ulong) offsetAndLength[2] << 16)
                         | ((ulong) offsetAndLength[1] << 24)
                         | ((ulong) offsetAndLength[0] << 32);
                Length = offsetAndLength[9]
                         | ((ulong) offsetAndLength[8] << 8)
                         | ((ulong) offsetAndLength[7] << 16)
                         | ((ulong) offsetAndLength[6] << 24)
                         | ((ulong) offsetAndLength[5] << 32);
            }
        }

        public void SetOffset(ulong offset)
        {
            Offset = offset;
        }

        public void SetLength(ulong length)
        {
            Length = length;
        }

        public byte[] Serialize()
        {
            byte[] OffsetAndLength = new byte[5 + 5];
            OffsetAndLength[0] = (byte)(Offset >> 32);
            OffsetAndLength[1] = (byte)(Offset >> 24);
            OffsetAndLength[2] = (byte)(Offset >> 16);
            OffsetAndLength[3] = (byte)(Offset >> 8);
            OffsetAndLength[4] = (byte)(Offset >> 0);
            
            OffsetAndLength[5] = (byte)(Length >> 32);
            OffsetAndLength[6] = (byte)(Length >> 24);
            OffsetAndLength[7] = (byte)(Length >> 16);
            OffsetAndLength[8] = (byte)(Length >> 8);
            OffsetAndLength[9] = (byte)(Length >> 0);

            return OffsetAndLength;
        }

        public override string ToString()
        {
            return $"{nameof(Offset)} {Offset} | {nameof(Length)} {Length}";
        }
    }
}