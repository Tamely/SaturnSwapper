module;

#include "Saturn/Defines.h"

export module Saturn.Core.UObject;

import Saturn.Reflection.FProperty;
import <string>;
import <vector>;
import <optional>;
import Saturn.Core.TObjectPtr;

export typedef TObjectPtr<class UObject> UObjectPtr;
export typedef TObjectPtr<class UClass> UClassPtr;
export typedef TObjectPtr<class UStruct> UStructPtr;

export class UObject : public std::enable_shared_from_this<UObject> {
public:
    UObject() = default;

    //friend class UZenPackage;
    //friend struct FUnversionedSerializer;

    enum EObjectFlags
    {
        // Do not add new flags unless they truly belong here. There are alternatives.
        // if you change any the bit of any of the RF_Load flags, then you will need legacy serialization
        RF_NoFlags					= 0x00000000,	///< No flags, used to avoid a cast

        // This first group of flags mostly has to do with what kind of object it is. Other than transient, these are the persistent object flags.
        // The garbage collector also tends to look at these.
        RF_Public					=0x00000001,	///< Object is visible outside its package.
        RF_Standalone				=0x00000002,	///< Keep object around for editing even if unreferenced.
        RF_MarkAsNative				=0x00000004,	///< Object (UField) will be marked as native on construction (DO NOT USE THIS FLAG in HasAnyFlags() etc)
        RF_Transactional			=0x00000008,	///< Object is transactional.
        RF_ClassDefaultObject		=0x00000010,	///< This object is used as the default template for all instances of a class. One object is created for each class
        RF_ArchetypeObject			=0x00000020,	///< This object can be used as a template for instancing objects. This is set on all types of object templates
        RF_Transient				=0x00000040,	///< Don't save object.

        // This group of flags is primarily concerned with garbage collection.
        RF_MarkAsRootSet			=0x00000080,	///< Object will be marked as root set on construction and not be garbage collected, even if unreferenced (DO NOT USE THIS FLAG in HasAnyFlags() etc)
        RF_TagGarbageTemp			=0x00000100,	///< This is a temp user flag for various utilities that need to use the garbage collector. The garbage collector itself does not interpret it.

        // The group of flags tracks the stages of the lifetime of a uobject
        RF_NeedInitialization		=0x00000200,	///< This object has not completed its initialization process. Cleared when ~FObjectInitializer completes
        RF_NeedLoad					=0x00000400,	///< During load, indicates object needs loading.
        RF_KeepForCooker			=0x00000800,	///< Keep this object during garbage collection because it's still being used by the cooker
        RF_NeedPostLoad				=0x00001000,	///< Object needs to be postloaded.
        RF_NeedPostLoadSubobjects	=0x00002000,	///< During load, indicates that the object still needs to instance subobjects and fixup serialized component references
        RF_NewerVersionExists		=0x00004000,	///< Object has been consigned to oblivion due to its owner package being reloaded, and a newer version currently exists
        RF_BeginDestroyed			=0x00008000,	///< BeginDestroy has been called on the object.
        RF_FinishDestroyed			=0x00010000,	///< FinishDestroy has been called on the object.

        // Misc. Flags
        RF_BeingRegenerated			=0x00020000,	///< Flagged on UObjects that are used to create UClasses (e.g. Blueprints) while they are regenerating their UClass on load (See FLinkerLoad::CreateExport()), as well as UClass objects in the midst of being created
        RF_DefaultSubObject			=0x00040000,	///< Flagged on subobject templates that were created in a class constructor, and all instances created from those templates
        RF_WasLoaded				=0x00080000,	///< Flagged on UObjects that were loaded
        RF_TextExportTransient		=0x00100000,	///< Do not export object to text form (e.g. copy/paste). Generally used for sub-objects that can be regenerated from data in their parent object.
        RF_LoadCompleted			=0x00200000,	///< Object has been completely serialized by linkerload at least once. DO NOT USE THIS FLAG, It should be replaced with RF_WasLoaded.
        RF_InheritableComponentTemplate = 0x00400000, ///< Flagged on subobject templates stored inside a class instead of the class default object, they are instanced after default subobjects
        RF_DuplicateTransient		=0x00800000,	///< Object should not be included in any type of duplication (copy/paste, binary duplication, etc.)
        RF_StrongRefOnFrame			=0x01000000,	///< References to this object from persistent function frame are handled as strong ones.
        RF_NonPIEDuplicateTransient	=0x02000000,	///< Object should not be included for duplication unless it's being duplicated for a PIE session
        // RF_Dynamic				=0x04000000,	///< Was removed along with bp nativization
        RF_WillBeLoaded				=0x08000000,	///< This object was constructed during load and will be loaded shortly
        RF_HasExternalPackage		=0x10000000,	///< This object has an external package assigned and should look it up when getting the outermost package
        // RF_Unused				=0x20000000,

        // RF_MirroredGarbage is mirrored in EInternalObjectFlags::Garbage because checking the internal flags is much faster for the Garbage Collector
        // while checking the object flags is much faster outside of it where the Object pointer is already available and most likely cached.
        RF_MirroredGarbage			=0x40000000,	///< Garbage from logical point of view and should not be referenced. This flag is mirrored in EInternalObjectFlags as Garbage for performance
        RF_AllocatedInSharedPage	=0x80000000,	///< Allocated from a ref-counted page shared with other UObjects
    };
protected:
    UClassPtr Class;
    UObjectPtr Outer;
    std::string Name;
    EObjectFlags ObjectFlags;
    std::vector<std::pair<const std::string&, TUniquePtr<class IPropValue>>> PropertyValues;

    template <typename T = UObject>
    __forceinline TObjectPtr<T> This() {
        return std::dynamic_pointer_cast<T>(shared_from_this());
    }
public:

    // Getters
    __forceinline std::string GetName() { return Name; }
    __forceinline UClassPtr GetClass() { return Class; }
    __forceinline UObjectPtr GetOuter() { return Outer; }

    // Setters
    __forceinline void SetName(std::string& Val) {
        Name = Val;
    }

    __forceinline void SetClass(UClassPtr Val) {
        Class = Val;
    }

    __forceinline void SetOuter(UObjectPtr Val) {
        Outer = Val;
    }

    // Flags
    __forceinline void SetFlags(EObjectFlags NewFlags) {
        ObjectFlags = static_cast<EObjectFlags>(ObjectFlags & NewFlags);
    }

    __forceinline void SetFlagsTo(EObjectFlags NewFlags) {
        ObjectFlags = NewFlags;
    }

    __forceinline EObjectFlags GetFlags() {
        return ObjectFlags;
    }

    __forceinline void ClearFlags(EObjectFlags NewFlags) {
        ObjectFlags = static_cast<EObjectFlags>(ObjectFlags & ~NewFlags);
    }

    __forceinline bool HasAnyFlags(EObjectFlags FlagsToCheck) const {
        return (ObjectFlags & FlagsToCheck) != 0;
    }

    __forceinline bool IsLoaded() {
        return ObjectFlags & RF_WasLoaded;
    }

    virtual void Load() {}
    virtual void Serialize(class FArchive& Ar);

    template <typename T>
    std::optional<T> TryGetProperty(std::string PropertyName) {
        for (auto& Prop : PropertyValues) {
            if (Prop.first == PropertyName) {
                return Prop.second.get()->TryGetValue<T>();
            }
        }

        return std::nullopt;
    }

    template <typename T>
    inline T GetProperty(std::string PropertyName) {
        return TryGetProperty<T>(PropertyName).value();
    }
};

export class UStruct : public UObject {
public:
    friend class UObject;
    friend class Mappings;

    ~UStruct();
private:
    UStructPtr Super;
    FProperty* PropertyLink = nullptr;
public:
    void SetSuper(UStructPtr Val);
    UStructPtr GetSuper();

    __forceinline FProperty* GetPropertyLink() {
        return PropertyLink;
    }

    void SerializeScriptProperties(class FArchive& Ar, UObjectPtr Object);
    TUniquePtr<IPropValue> SerializeItem(class FArchive& Ar);
};

export class UClass : public UStruct {
public:
    friend class UObject;
};