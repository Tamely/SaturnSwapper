using UAssetAPI.PropertyTypes.Objects;

namespace UAssetAPI.PropertyFactories;

public class FloatFactory
{
    public static FloatPropertyData Create(float value)
    {
        FloatPropertyData data = new FloatPropertyData
        {
            Value = value
        };

        return data;
    }
}