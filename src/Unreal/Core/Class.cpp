import Saturn.Core.UObject;
import Saturn.Readers.FArchive;

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

void UStruct::SerializeScriptProperties(FArchive& Ar, UObjectPtr Object) {
    // TODO: Serialize properties
}