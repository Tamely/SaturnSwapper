using UAssetAPI.PropertyTypes.Objects;

namespace UAssetAPI.PropertyFactories;

public class ByteArrayPropertyData : PropertyData
{
    public byte[] Value;

    public override byte[] Serialize(UnrealPackage Asset)
    {
        return Value;
    }
}

public class ByteArrayFactory
{
    public static ByteArrayPropertyData Create(byte[] value)
    {
        ByteArrayPropertyData data = new ByteArrayPropertyData
        {
            Value = value
        };

        return data;
    }
}