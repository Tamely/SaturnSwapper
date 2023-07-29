using System.Collections.Generic;
using CUE4Parse.UE4.Objects.UObject;

namespace Saturn.Backend.Data.Swapper;

public interface INameMap
{
    IReadOnlyList<FNameEntrySerialized> GetNameMapIndexList();
    void ClearNameIndexList();
    void SetNameReference(int index, FNameEntrySerialized value);
    FNameEntrySerialized GetNameReference(int index);
    bool ContainsNameReference(FNameEntrySerialized search);
    int SearchNameReference(FNameEntrySerialized search);
    int AddNameReference(FNameEntrySerialized name, bool forceAddDuplicates = false);
}