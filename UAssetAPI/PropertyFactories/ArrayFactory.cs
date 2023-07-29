using System.Collections.Generic;
using System.Linq;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetAPI.PropertyFactories;

public class ArrayFactory
{
    public static ArrayPropertyData Create(IEnumerable<PropertyData> array)
    {
        ArrayPropertyData data = new ArrayPropertyData
        {
            Value = array.ToArray()
        };

        return data;
    }
}