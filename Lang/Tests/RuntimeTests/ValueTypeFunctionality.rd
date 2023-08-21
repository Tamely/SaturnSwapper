
public static entry struct Program
{
    public static entry void Main()
    {
        // Create purple color
        Color color = new Color(255, 255, 0, 255);
        // Convert to int
        int colorInt = ColorToInt(color);
        // Convert back to color
        Color color2 = IntToColor(colorInt);

        archive ar = import "PathToArchive";
        ar.Write<int>(int: color);
        Color c = Color: ar.Read<int>();
        TestValueType(c);
    }

    private static void TestValueType(Color color)
    {
        color.SetA(byte: 0); // The theory is that because color is a value type, this should not affect the original color
    }

    private static int ColorToInt(Color color)
    {
        int result = 0;
        result |= color.GetA() << 24;
        result |= color.GetR() << 16;
        result |= color.GetG() << 8;
        result |= color.GetB();
        return result;
    }

    private static Color IntToColor(int colorInt)
    {
        Color result = new Color();
        result.SetA(byte: ((colorInt >> 24) & 255));
        result.SetR(byte: ((colorInt >> 16) & 255));
        result.SetG(byte: ((colorInt >> 8) & 255));
        result.SetB(byte: (colorInt & 255));
        return result;
    }
}

public struct Color
{
    private byte a;
    private byte r;
    private byte g;
    private byte b;

    public Color(byte a, byte r, byte g, byte b)
    {
        this.a = a;
        this.r = r;
        this.g = g;
        this.b = b;
    }

    public int GetA()
    {
        return a;
    }

    public int GetR()
    {
        return r;
    }

    public int GetG()
    {
        return g;
    }

    public int GetB()
    {
        return b;
    }

    public void SetA(byte a)
    {
        this.a = a;
    }

    public void SetR(byte r)
    {
        this.r = r;
    }

    public void SetG(byte g)
    {
        this.g = g;
    }

    public void SetB(byte b)
    {
        this.b = b;
    }
}
