using UAssetAPI.PropertyTypes.Objects;

namespace UAssetAPI.PropertyFactories;

public class IntFactory
{
    public static IntPropertyData Create(int value)
    {
        IntPropertyData data = new IntPropertyData
        {
            Value = value
        };

        return data;
    }
}