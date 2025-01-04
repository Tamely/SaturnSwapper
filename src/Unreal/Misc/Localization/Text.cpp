import Saturn.Localization.Text;

import <vector>;
import <string>;
import <cstdint>;
import <optional>;
import <memory>;

import Saturn.Readers.FArchive;

typedef std::vector<std::pair<std::string, struct FFormatArgumentValue>> FFormatNamedArguments;
typedef std::vector<struct FFormatArgumentValue> FFormatOrderedArguments;

struct FNumberFormattingOptions {
    FNumberFormattingOptions() = default;

    enum ERoundingMode {
        HalfToEven,
        HalfFromZero,
        HalfToZero,
        FromZero,
        ToZero,
        ToNegativeInfinity,
        ToPositiveInfinity
    };

    bool AlwaysSign;
    bool UseGrouping;
    ERoundingMode RoundingMode;
    int32_t MinimumIntegralDigits;
    int32_t MaximumIntegralDigits;
    int32_t MinimumFractionDigits;
    int32_t MaximumFractionDigits;

    friend FArchive& operator<<(FArchive& Ar, FNumberFormattingOptions& Value) {
        Ar << Value.AlwaysSign;
        Ar << Value.UseGrouping;

        int8_t ByteRoundingMode;
        Ar << ByteRoundingMode;
        Value.RoundingMode = static_cast<ERoundingMode>(ByteRoundingMode);

        Ar << Value.MinimumIntegralDigits;
        Ar << Value.MaximumIntegralDigits;
        Ar << Value.MinimumFractionDigits;
        Ar << Value.MaximumFractionDigits;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FNumberFormattingOptions& Value) {
        Ar >> Value.AlwaysSign;
        Ar >> Value.UseGrouping;

        int8_t ByteRoundingMode = static_cast<int8_t>(Value.RoundingMode);
        Ar >> ByteRoundingMode;

        Ar >> Value.MinimumIntegralDigits;
        Ar >> Value.MaximumIntegralDigits;
        Ar >> Value.MinimumFractionDigits;
        Ar >> Value.MaximumFractionDigits;

        return Ar;
    }
};

namespace EFormatArgumentType {
    enum Type {
        Int,
        UInt,
        Float,
        Double,
        Text,
        Gender
    };
}

namespace EDateTimeStyle {
    enum Type {
        Default,
        Short,
        Medium,
        Long,
        Full,
        Custom
    };
}

struct FFormatArgumentValue {
    friend FArchive& operator<<(FArchive& Ar, FFormatArgumentValue& Value) {
        int8_t TypeAsInt8;
        Ar << TypeAsInt8;

        Value.Type = (EFormatArgumentType::Type)TypeAsInt8;

        switch (Value.Type) {
            case EFormatArgumentType::Double: {
                Ar << Value.DoubleValue;
                break;
            }
            case EFormatArgumentType::Float: {
                Ar << Value.FloatValue;
                break;
            }
            case EFormatArgumentType::Int: {
                Ar << Value.IntValue;
                break;
            }
            case EFormatArgumentType::UInt: {
                Ar << Value.UIntValue;
                break;
            }
            case EFormatArgumentType::Text: {
                Value.TextValue = FText();
                Ar << Value.TextValue.value();
                break;
            }
        }

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FFormatArgumentValue& Value) {
        int8_t TypeAsInt8 = (int8_t)Value.Type;
        Ar >> TypeAsInt8;

        switch (Value.Type) {
            case EFormatArgumentType::Double: {
                Ar >> Value.DoubleValue;
                break;
            }
            case EFormatArgumentType::Float: {
                Ar >> Value.FloatValue;
                break;
            }
            case EFormatArgumentType::Int: {
                Ar >> Value.IntValue;
                break;
            }
            case EFormatArgumentType::UInt: {
                Ar >> Value.UIntValue;
                break;
            }
            case EFormatArgumentType::Text: {
                Ar >> Value.TextValue.value();
                break;
            }
        }

        return Ar;
    }

    std::string ToString() {
        switch (Type) {
            case EFormatArgumentType::Double: {
                return std::to_string(DoubleValue);
            }
            case EFormatArgumentType::Float: {
                return std::to_string(FloatValue);
            }
            case EFormatArgumentType::Int: {
                return std::to_string(IntValue);
            }
            case EFormatArgumentType::UInt: {
                return std::to_string(UIntValue);
            }
            case EFormatArgumentType::Text: {
                return TextValue.value().ToString();
            }
            default: return {};
        }
    }

    EFormatArgumentType::Type Type;

    union {
        int64_t IntValue;
        uint64_t UIntValue;
        float FloatValue;
        double DoubleValue;
    };

    std::optional<FText> TextValue;
};

struct FFormatArgumentData {
    friend FArchive& operator<<(FArchive& Ar, FFormatArgumentData& Value) {
        Ar << Value.ArgumentName;
        Ar << Value.ArgumentValue;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FFormatArgumentData& Value) {
        Ar >> Value.ArgumentName;
        Ar >> Value.ArgumentValue;

        return Ar;
    }

    std::string ArgumentName;
    FFormatArgumentValue ArgumentValue;
};

enum class ETextHistoryType : int8_t {
    None = -1,
    Base = 0,
    NamedFormat,
    OrderedFormat,
    ArgumentFormat,
    AsNumber,
    AsPercent,
    AsCurrency,
    AsDate,
    AsTime,
    AsDateTime,
    Transform,
    StringTableEntry,
    TextGenerator
};

class FTextBaseHistory : public ITextData {
public:
    FTextBaseHistory() = default;

    FTextBaseHistory(std::string& InSourceString)
        : SourceString(InSourceString), TextId({}) {}

    FTextBaseHistory(FTextId& InTextId, std::string& InSourceString)
        : TextId(InTextId), SourceString(InSourceString) {}
    
    __forceinline std::string& GetString() override {
        return SourceString;
    }

    __forceinline FTextId& GetTextId() {
        return TextId;
    }

    void Serialize(FArchive& Ar) override {
        FTextKey Namespace;
        Namespace.SerializeAsString(Ar);

        FTextKey Key;
        Key.SerializeAsString(Ar);

        Ar << SourceString;

        TextId = { Namespace, Key };
    }

    void Write(FArchive& Ar) override {
        if (TextId == FTextId()) {
            bool bHasCultureInvariantString = !SourceString.empty();
            Ar >> bHasCultureInvariantString;

            if (bHasCultureInvariantString) {
                Ar >> SourceString;
            }
        }
        else {
            TextId.Namespace.WriteAsString(Ar);
            TextId.Key.WriteAsString(Ar);

            Ar >> SourceString;
        }
    }
private:
    FTextId TextId;
    std::string SourceString;
};

class FTextNamedFormat : public ITextData {
public:
    __forceinline void Serialize(FArchive& Ar) override {
        Ar << Text;
        Ar << Arguments;
    }

    __forceinline void Write(FArchive& Ar) override {
        Ar >> Text;
        Ar >> Arguments;
    }

    __forceinline std::string& GetString() override {
        return Text.ToString();
    }
    
    FText Text;
    FFormatNamedArguments Arguments;
};

class FTextOrderedFormat : public ITextData {
public:
    __forceinline void Serialize(FArchive& Ar) override {
        Ar << Text;
        Ar << Arguments;
    }

    __forceinline void Write(FArchive& Ar) override {
        Ar >> Text;
        Ar >> Arguments;
    }

    __forceinline std::string& GetString() override {
        return Text.ToString();
    }
    
    FText Text;
    FFormatOrderedArguments Arguments;
};

class FTextArgumentDataFormat : public ITextData {
public:
    __forceinline void Serialize(FArchive& Ar) override {
        Ar << Text;
        Ar << Arguments;
    }

    __forceinline void Write(FArchive& Ar) override {
        Ar >> Text;
        Ar >> Arguments;
    }

    __forceinline std::string& GetString() override {
        return Text.ToString();
    }
    
    FText Text;
    std::vector<FFormatArgumentData> Arguments;
};

class FTextFormatNumber : public ITextData {
public:
    void Serialize(FArchive& Ar) override {
        Ar << SourceValueRaw;
        SourceValue = SourceValueRaw.ToString();

        Ar << bHasFormatOptions;

        if (bHasFormatOptions) {
            Ar << FormattingOptions;
        }

        Ar << CultureName;
    }

    void Write(FArchive& Ar) override {
        Ar >> SourceValueRaw;

        Ar >> bHasFormatOptions;

        if (bHasFormatOptions) {
            Ar >> FormattingOptions;
        }

        Ar >> CultureName;
    }

    __forceinline std::string& GetString() override {
        return SourceValue;
    }

    std::string SourceValue;
    FFormatArgumentValue SourceValueRaw;
    bool bHasFormatOptions;
    FNumberFormattingOptions FormattingOptions;
    std::string CultureName;
};

class FTextCurrency : public FTextFormatNumber {
public:
    FTextCurrency() : FTextFormatNumber() {}
    void Serialize(FArchive& Ar) override {
        Ar << CurrencyCode;
        FTextFormatNumber::Serialize(Ar);
    }

    __forceinline void Write(FArchive& Ar) override {
        Ar >> CurrencyCode;
        FTextFormatNumber::Write(Ar);
    }

    std::string CurrencyCode;
};

class FTextDate : public ITextData {
public:
    void Serialize(FArchive& Ar) override {
        Ar << SourceDateTime;

        int8_t DateStyleInt8;
        Ar << DateStyleInt8;
        DateStyle = static_cast<EDateTimeStyle::Type>(DateStyleInt8);

        Ar << TimeZone;

        Ar << TargetCulture;
    }

    void Write(FArchive& Ar) override {
        Ar >> SourceDateTime;

        int8_t DateStyleInt8 = static_cast<int8_t>(DateStyle);
        Ar >> DateStyleInt8;

        Ar >> TimeZone;

        Ar >> TargetCulture;
    }

    __forceinline std::string& GetString() override {
        std::string TempString("TODO: Return a formatted date from FText.");
        return TempString;
    }

    int64_t SourceDateTime;
    EDateTimeStyle::Type DateStyle;
    std::string TimeZone;
    std::string TargetCulture;
};

class FTextDateTime : public ITextData {
public:
    void Serialize(FArchive& Ar) override {
        Ar << SourceDateTime;

        int8_t DateStyleInt8;
        Ar << DateStyleInt8;
        DateStyle = static_cast<EDateTimeStyle::Type>(DateStyleInt8);

        int8_t TimeStyleInt8;
        Ar << TimeStyleInt8;
        TimeStyle = static_cast<EDateTimeStyle::Type>(TimeStyleInt8);

        if (DateStyle == EDateTimeStyle::Custom) {
            Ar << CustomPattern;
        }

        Ar << TimeZone;
        Ar << TargetCulture;
    }

    void Write(FArchive& Ar) override {
        Ar >> SourceDateTime;

        int8_t DateStyleInt8 = static_cast<int8_t>(DateStyle);
        Ar >> DateStyleInt8;

        int8_t TimeStyleInt8 = static_cast<int8_t>(TimeStyle);;
        Ar >> TimeStyleInt8;

        if (DateStyle == EDateTimeStyle::Custom) {
            Ar >> CustomPattern;
        }

        Ar >> TimeZone;
        Ar >> TargetCulture;
    }

    __forceinline std::string& GetString() override {
        std::string TempString("TODO: Return a formatted datetime from FText.");
        return TempString;
    }

    int64_t SourceDateTime;
    EDateTimeStyle::Type DateStyle;
    EDateTimeStyle::Type TimeStyle;
    std::string CustomPattern;
    std::string TimeZone;
    std::string TargetCulture;
};

class FTextTransform : public ITextData {
public:
    enum class ETransformType : uint8_t {
        ToLower = 0,
        ToUpper,
    };

    void Serialize(FArchive& Ar) override {
        Ar << SourceText;
        
        uint8_t TransformTypeUInt8;
        Ar << TransformTypeUInt8;

        TransformType = static_cast<ETransformType>(TransformTypeUInt8);
    }

    void Write(FArchive& Ar) override {
        Ar >> SourceText;
        
        uint8_t TransformTypeUInt8 = static_cast<uint8_t>(TransformType);
        Ar >> TransformTypeUInt8;
    }

    __forceinline std::string& GetString() override {
        return SourceText.ToString();
    }

    FText SourceText;
    ETransformType TransformType;
};

FArchive& operator<<(FArchive& Ar, FText& Value) {
    Ar << Value.Flags;
    Ar << Value.HistoryType;

    switch ((ETextHistoryType)Value.HistoryType) {
        case ETextHistoryType::Base: {
            Value.Data = std::make_unique<FTextBaseHistory>();
            break;
        }
        case ETextHistoryType::NamedFormat: {
            Value.Data = std::make_unique<FTextNamedFormat>();
            break;
        }
        case ETextHistoryType::OrderedFormat: {
            Value.Data = std::make_unique<FTextOrderedFormat>();
            break;
        }
        case ETextHistoryType::ArgumentFormat: {
            Value.Data = std::make_unique<FTextArgumentDataFormat>();
            break;
        }
        case ETextHistoryType::AsPercent:
        case ETextHistoryType::AsNumber: {
            Value.Data = std::make_unique<FTextFormatNumber>();
            break;
        }
        case ETextHistoryType::AsCurrency: {
            Value.Data = std::make_unique<FTextCurrency>();
            break;
        }
        case ETextHistoryType::AsTime:
        case ETextHistoryType::AsDate: {
            Value.Data = std::make_unique<FTextDate>();
            break;
        }
        case ETextHistoryType::AsDateTime: {
            Value.Data = std::make_unique<FTextDateTime>();
            break;
        }
        case ETextHistoryType::Transform: {
            Value.Data = std::make_unique<FTextTransform>();
            break;
        }
        case ETextHistoryType::StringTableEntry:
        case ETextHistoryType::TextGenerator: {
            //LOG_ERROR("Unsupported Text History type. {0}", Value.HistoryType);
            return Ar;
        }
        default: {
            bool bHasCultureInvariantString = false;
            Ar << bHasCultureInvariantString;

            if (bHasCultureInvariantString) {
                std::string CultureInvariantString;
                Ar << CultureInvariantString;

                Value.Data = std::make_unique<FTextBaseHistory>(CultureInvariantString);
            }
            else {
                Value.Data = std::make_unique<FTextBaseHistory>();
            }

            return Ar;
        }
    }

    Value.Data->Serialize(Ar);
    return Ar;
}

FArchive& operator>>(FArchive& Ar, FText& Value) {
    Ar >> Value.Flags;
    Ar >> Value.HistoryType;

    Value.Data->Write(Ar);
    return Ar;
}

void FTextKey::Serialize(FArchive& Ar, ELocResVersion& Ver) {
    if (Ver >= ELocResVersion::Optimized_CityHash64_UTF16) {
        Ar << StrHash;
    }
    else if (Ver == ELocResVersion::Optimized_CRC32) {
        Ar << Pad;
    }

    Ar << Str;
}

void FTextKey::Write(FArchive& Ar, ELocResVersion& Ver) {
    if (Ver >= ELocResVersion::Optimized_CityHash64_UTF16) {
        Ar >> StrHash;
    }
    else if (Ver == ELocResVersion::Optimized_CRC32) {
        Ar >> Pad;
    }

    Ar >> Str;
}

void FTextKey::SerializeAsString(FArchive& Ar) {
    Ar << Str;
}

void FTextKey::WriteAsString(FArchive& Ar) {
    Ar >> Str;
}

FArchive& operator<<(FArchive& Ar, FTextLocalizationResourceString& A) {
    Ar << A.String;
    Ar << A.Pad;

    return Ar;
}

FArchive& operator>>(FArchive& Ar, FTextLocalizationResourceString& A) {
    Ar >> A.String;
    Ar >> A.Pad;

    return Ar;
}

uint32_t HashCombineFast(uint32_t A, uint32_t B) {
    return A ^ (B + 0x9e3779b9 + (A << 6) + (A >> 2));
}

size_t hash_value(const FTextId& i) {
    return HashCombineFast(hash_value(i.Key), hash_value(i.Namespace));
}