export module Saturn.Reflection.PropertyValue;

export import <optional>;
import <string>;
import <vector>;

import Saturn.Core.TObjectPtr;

template <class, template <class> class>
struct is_t : public std::false_type {};

template <class T, template <class> class U>
struct is_t<U<T>, U> : public std::true_type {};

template <typename T>
struct is_vector : std::false_type {};

template <typename T, typename Alloc>
struct is_vector<std::vector<T, Alloc>> : std::true_type {};

export enum class EPropertyType : uint8_t {
	ByteProperty,
	BoolProperty,
	IntProperty,
	FloatProperty,
	ObjectProperty,
	NameProperty,
	DelegateProperty,
	DoubleProperty,
	ArrayProperty,
	StructProperty,
	StrProperty,
	TextProperty,
	InterfaceProperty,
	MulticastDelegateProperty,
	WeakObjectProperty,
	LazyObjectProperty,
	AssetObjectProperty,
	SoftObjectProperty,
	UInt64Property,
	UInt32Property,
	UInt16Property,
	Int64Property,
	Int16Property,
	Int8Property,
	MapProperty,
	SetProperty,
	EnumProperty,
	FieldPathProperty,

    OptionalProperty,
    Utf8StrProperty,
    AnsiStrProperty,

	Unknown = 0xFF
};

export template <typename T>
constexpr EPropertyType GetPropertyType() {
    if constexpr (std::is_same<T, uint8_t>()) return EPropertyType::ByteProperty;
    if constexpr (std::is_same<T, bool>()) return EPropertyType::BoolProperty;
    if constexpr (std::is_same<T, float>()) return EPropertyType::FloatProperty;
    if constexpr (is_t<T, TObjectPtr>::value) return EPropertyType::ObjectProperty;
    if constexpr (std::is_same<T, class FName>()) return EPropertyType::NameProperty;
    if constexpr (std::is_same<T, class FScriptDelegate>()) return EPropertyType::DelegateProperty;
    if constexpr (std::is_same<T, class FSoftObjectPath>()) return EPropertyType::SoftObjectProperty;
    if constexpr (std::is_same<T, double>()) return EPropertyType::DoubleProperty;
    if constexpr (std::is_same<T, std::string>()) return EPropertyType::StrProperty;
    if constexpr (std::is_same<T, class Text>()) return EPropertyType::TextProperty;
    if constexpr (std::is_same<T, class FMulticastScriptDelegate>()) return EPropertyType::MulticastDelegateProperty;
    if constexpr (std::is_same<T, uint64_t>()) return EPropertyType::UInt64Property;
    if constexpr (std::is_same<T, uint32_t>()) return EPropertyType::UInt32Property;
    if constexpr (std::is_same<T, uint16_t>()) return EPropertyType::UInt16Property;
    if constexpr (std::is_same<T, int64_t>()) return EPropertyType::Int64Property;
    if constexpr (std::is_same<T, int32_t>()) return EPropertyType::Int32Property;
    if constexpr (std::is_same<T, int16_t>()) return EPropertyType::Int16Property;
    if constexpr (std::is_same<T, int8_t>()) return EPropertyType::Int8Property;
    if constexpr (is_vector<T>::value) return EPropertyType::ArrayProperty;
    if constexpr (std::is_class<T>::value) return EPropertyType::StructProperty;

    return EPropertyType::Unknown;
}

export class IPropValue {
protected:
    int ValueTypeSize = 0;
public:
    enum class ESerializationMode : uint8_t {
        Zero,
        Normal,
        Map
    };

    virtual bool IsAcceptableType(EPropertyType Type) = 0;
    virtual void PlaceValue(EPropertyType Type, void* OutBuffer) = 0;
    virtual void Write(class FArchive& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) = 0;

    template <typename T>
    std::optional<T> TryGetValue() {
        constexpr auto RuntimeType = GetPropertyType<T>();

        if (!IsAcceptableType(RuntimeType)) {
            return std::nullopt;
        }

        ValueTypeSize = sizeof(T);
        T Val;

        if constexpr (RuntimeType == EPropertyType::ArrayProperty) {
            using ElementType = typename T::value_type;
            ValueTypeSize = sizeof(ElementType);

            PlaceValue(GetPropertyType<ElementType>(), &Val);
        }
        else {
            PlaceValue(RuntimeType, &Val);
        }

        return std::optional<T>(Val);
    }
};