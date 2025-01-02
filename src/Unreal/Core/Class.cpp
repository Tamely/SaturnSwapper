import Saturn.Core.UObject;
import Saturn.Readers.ZenPackageReader;

UStruct::~UStruct() {
    while (PropertyLink) {
        auto LinkCopy = PropertyLink;
        PropertyLink = PropertyLink->GetNext();
        delete LinkCopy;
    }
}

UStructPtr UStruct::GetSuper() {
    return Super;
}

void UStruct::SetSuper(UStructPtr Val) {
    Super = Val;
}

void UStruct::SerializeScriptProperties(FZenPackageReader& Ar, UObjectPtr Object) {
    Ar.LoadProperties(This<UStruct>(), Object);
}