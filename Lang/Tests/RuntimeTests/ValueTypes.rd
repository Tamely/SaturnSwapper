
public static entry struct Program
{
    public static entry void Main()
    {
        Color red = new Color(255, 255, 0, 0);
        Color green = new Color(255, 0, 255, 0);
        Color blue = new Color(255, 0, 0, 255);
        Color purple = new Color(255, 255, 0, 255);
        Color yellow = new Color(255, 255, 255, 0);
        Color cyan = new Color(255, 0, 255, 255);
        Color white = new Color(255, 255, 255, 255);
        Color black = new Color(255, 0, 0, 0);

        Color[] colors = new Color[8];
        colors[0] = red;
        colors[1] = green;
        colors[2] = blue;
        colors[3] = purple;
        colors[4] = yellow;
        colors[5] = cyan;
        colors[6] = white;
        colors[7] = black;

        for (int i = 0; i < colors.Length(); i++)
        {
            InvertColor(colors[i]); // In theory, color[i] sould not change because it is passed by value
        }
    }

    public static void InvertColor(Color color)
    {
        color.SetR(byte: (255 - color.GetR()));
        color.SetG(byte: (255 - color.GetG()));
        color.SetB(byte: (255 - color.GetB()));
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
