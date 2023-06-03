namespace Saturn.Backend.Data.Compression
{
    public abstract class CompressionBase
    {
        public abstract byte[] Decompress(byte[] data, int decompressedSize);
        public abstract byte[] Compress(byte[] buffer);
    }
}
