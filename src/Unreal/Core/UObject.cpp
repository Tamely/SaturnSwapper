import Saturn.Core.UObject;
import Saturn.Readers.FArchive;

void UObject::Serialize(FArchive& Ar) {
    if (Class) {
        Class->SerializeScriptProperties(Ar, This());
    }
}