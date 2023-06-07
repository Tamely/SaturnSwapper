#include "TestStructs.rd"

sign: "Author", "Fin"

from_ar = import "Path/To/Asset"
to_ar = import "Path/To/OtherAsset"

from_ar.Write<byte>(100)
from_ar.Seek(100, seekorigin.Current)
from_ar.Swap(to_ar)
int x = from_ar.Read<int>()
list<int> intList = new list<int>()
intList.Add(10)
intList.Remove(10)
intList.Add(100000)

list<string> stringList = new list<string>()
stringList.Add("Hello")
stringList.Add(" ")
stringList.Add("World!")