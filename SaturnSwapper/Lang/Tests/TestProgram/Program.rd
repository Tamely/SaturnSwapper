#include "Templates.rd"

public entry struct Program
{
    public static entry void Main()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);

        List<int> intList = new List<int>();
        intList.Add(1);
        intList.Add(2);
        intList.Add(3);

        List<int> intList2 = new List<int>();
        intList2.Add(1);
        intList2.Add(2);
        intList2.Add(3);
        intList2.Add(4);
        intList2.Add(5);
        intList2.Add(6);

        intList.RemoveAt(1);
        intList.AddRange(intList2);

        bool contains = intList.Contains(3); // true
        int length = intList.Count(); // 8

        List<int> customList = new List<int>();
        customList.Add(1);
        customList.Add(2);
        customList.Add(3);
        customList.RemoveAt(1); // Remove 2
        customList.Remove(3);
        List<int> addRangeList = new List<int>();
        addRangeList.Add(2);
        addRangeList.Add(3);
        addRangeList.Add(4);
        addRangeList.Add(5);
        addRangeList.Add(6);

        customList.AddRange(addRangeList); // Add 4, 5, 6

        List<MyEnum> enumList = new List<MyEnum>();
        enumList.Add(MyEnum.A);
        enumList.Add(MyEnum.B);
        enumList.Add(MyEnum.C);
    }
}
