using System;
using System.Collections.Generic;
using System.Linq;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetAPI.PropertyFactories;

public class ArrayFactory
{
    public static ArrayPropertyData Create(IEnumerable<PropertyData> array)
    {
        if (FactoryUtils.ASSET == null)
            throw new Exception("FactoryUtils.ASSET is null. Did you forget to set it?");
        
        ArrayPropertyData data = new ArrayPropertyData
        {
            Value = array.ToArray()
        };

        return data;
    }
}