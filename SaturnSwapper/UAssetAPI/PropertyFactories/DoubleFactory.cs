using UAssetAPI.PropertyTypes.Objects;

namespace UAssetAPI.PropertyFactories;

public class DoubleFactory
{
    public static DoublePropertyData Create(double value)
    {
        DoublePropertyData data = new DoublePropertyData
        {
            Value = value
        };

        return data;
    }
}