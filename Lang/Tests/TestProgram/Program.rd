#include "Templates.rd"

public struct Program
{
    public static void Main()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);

        list<int> intList = new list<int>();
        intList.Add(1);
        intList.Add(2);
        intList.Add(3);

        list<int> intList2 = new list<int>();
        intList2.Add(1);
        intList2.Add(2);
        intList2.Add(3);
        intList2.Add(4);
        intList2.Add(5);
        intList2.Add(6);

        intList.RemoveAt(1);
        intList.AddRange(intList2);

        bool contains = intList.Contains(3); // true
        int length = intList.Length(); // 8

        List<int> customList = new List<int>();

        list<MyEnum> enumList = new list<MyEnum>();
        enumList.Add(MyEnum.A);
        enumList.Add(MyEnum.B);
        enumList.Add(MyEnum.C);
    }
}
