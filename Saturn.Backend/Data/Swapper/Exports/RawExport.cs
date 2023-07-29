using GenericReader;

namespace Saturn.Backend.Data.Swapper.Exports;

public class RawExport : IExportBase
{
    protected readonly GenericBufferReader _reader;
    public RawExport(byte[] data)
    {
        _reader = new GenericBufferReader(data);
        _reader.Position = 0;
    }

    public virtual void Read(int nextStarting = 0)
    {
        // ignore, this will be implemented in a little bit
    }

    public virtual byte[] Serialize()
    {
        _reader.Position = 0;
        return _reader.ReadArray<byte>(_reader.Size);
    }
}