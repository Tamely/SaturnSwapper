import Saturn.Core.UObject;
import Saturn.Readers.ZenPackageReader;

void UObject::Serialize(FZenPackageReader& Ar) {
    if (Class) {
        Class->SerializeScriptProperties(Ar, This());
    }
}