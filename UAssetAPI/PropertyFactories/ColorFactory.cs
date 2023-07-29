using System.Drawing;
using UAssetAPI.PropertyTypes.Structs;

namespace UAssetAPI.PropertyFactories;

public class ColorFactory
{
    public static LinearColorPropertyData Create(Color color)
    {
        LinearColorPropertyData data = new LinearColorPropertyData();
        LinearColor value = new LinearColor
        {
            R = color.R,
            G = color.G,
            B = color.B,
            A = color.A
        };

        data.Value = value;
        return data;
    }
}