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

template Template<T>
{
    T Field
    
    T Method(T param)
    {
        T x = default: T
    }
}

struct TemplateMethods
{
    int Field

    static template list<T> TemplateMethod<T>(T param)
    {
        T x = default: T
    }
}

/*
template Template<T>
{
    T Field
    
    T Method(T param)
    {
        T x = default: T
    }
}

var x = new Template<int>()
int i = x.Method(10)

struct Template`int
{
    int Field

    int Method(int param)
    {
        int x = default: T
    }
}

var x = new Template`int()
int i = x.Method(10)



struct TemplateMethods
{
    int Field

    static template list<T> TemplateMethod<T>(T param)
    {
        T x = default: T
    }
}

var i = TemplateMethods.TemplateMethod<float>(10.5)

struct TemplateMethods
{
    int Field

    static template list<T> TemplateMethod<T>(T param)
    {
        T x = default: T
    }

    static list<float> TemplateMethod`float(float param)
    {
        float x = default: float
    }
}

var i = TemplateMethods.TemplateMethod`float(10.5)










//
*/
