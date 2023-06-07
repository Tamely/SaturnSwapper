struct Vector2D
{
    int x
    int y
    int z
    static int a = 10

    Vector2D(int x, int y, int z)
    {
        this.x = x
        this.y = y
        this.z = z
    }

    double Add(double x, double y)
    {
        double result = x + y
    }
}

enum Enum
{
    A = 1,
    B,
    C = 3
}
