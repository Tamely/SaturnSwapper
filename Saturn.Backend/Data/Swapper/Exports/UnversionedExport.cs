using System.Collections.Generic;
using Saturn.Backend.Data.Swapper.Properties;
using Saturn.Backend.Data.Swapper.Serialization;
using Saturn.Backend.Data.Swapper.Unversioned;

namespace Saturn.Backend.Data.Swapper.Exports;

public class UnversionedExport : RawExport
{
    public List<PropertyData> Properties;
    public UAsset Owner;
    
    public UnversionedExport(byte[] data, UAsset owner) : base(data)
    {
        Owner = owner;
    }

    public override void Read(int nextStarting = 0)
    {
        Properties = new List<PropertyData>();
        PropertyData bit;

        _reader.Position = 0;
        var unversionedHeader = new FUnversionedHeader(_reader);
        //while ((bit = MainSerializer.Read(_reader, null, GetClassTypeForAncestry(Owner), unversionedHeader, true)) != null)
        {
            //Properties.Add(bit);
        }
    }

    public override byte[] Serialize()
    {
        List<byte> data = new();
        //byte[] header = MainSerializer.GenerateUnversionedHeader(ref Properties, GetClassTypeForAncestry(Owner), Owner);

        //data.AddRange(header);
        for (int i = 0; i < data.Count; i++)
        {
            PropertyData current = Properties[i];
            //data.AddRange(MainSerializer.Serialize(current, true));
        }

        return data.ToArray();
    }
}