template List<T>
{
    T[] _array

    List(int capacity)
    {
        _array = new T[capacity]
    }

    void Set(int index, T value)
    {
        _array[index] = value 
    }
}

struct Person
{
    Name name
    int age

    Person(Name name, int age)
    {
        this.name = name
        this.age = age
    }

    string GetFirstName()
    {
        string first = name.firstName
    }
}

struct Name
{
    string firstName
    string lastName

    Name(string first, string last)
    {
        firstName = first
        lastName = last
    }
}

struct Program
{
    static void Main()
    {
        List<int> intList = new List<int>(3)
        intList.Set(0, 10)
        intList.Set(1, 20)
        intList.Set(2, 30)

        Person person = new Person(new Name("John", "Doe"), 45)
    }
}