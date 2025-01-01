export module Saturn.Asset.BulkDataCookedIndex;

import <cctype>;
import <string>;
import <sstream>;
import <cstdint>;
import <algorithm>;
import <iomanip>;

import Saturn.Readers.FArchive;

export enum class EBulkDataPayloadType : uint8_t {
    Inline,             // Stored inside the export data in .uexp
    AppendToExports,    // Stored after the export data in .uexp
    BulkSegment,        // Stored in .ubulk
    Optional,           // Stored in .uptnl
    MemoryMapped        // Stored in .m.bulk
};

export class FBulkDataCookedIndex {
public:
    // It is likely that we will want to expand the number of bits that this system currently uses whenn addressed
    // via FIoChunkIds in the future. The following constants and aliases make it easier to track places in the
    // code base that make assumptions about this so we can safely update them all at once.
    using ValueType = uint8_t;
    constexpr static int32_t MAX_DIGITS = 3;

    static const FBulkDataCookedIndex Default;

    FBulkDataCookedIndex() = default;
    explicit FBulkDataCookedIndex(ValueType InValue)
        : Value(InValue) {}

    ~FBulkDataCookedIndex() = default;

    bool IsDefault() const {
        return Value == 0;
    }

    std::string GetAsExtension() const {
        if (IsDefault()) {
            return "";
        }
        else {
            std::ostringstream oss;
            oss << "." << std::setfill('0') << std::setw(3) << static_cast<unsigned>(Value);
            return oss.str();
        }
    }

    ValueType GetValue() const {
        return Value;
    }

    bool operator==(const FBulkDataCookedIndex& Other) const {
        return Value == Other.Value;
    }

    bool operator<(const FBulkDataCookedIndex& Other) const {
        return Value < Other.Value;
    }

    friend FArchive& operator<<(FArchive& Ar, FBulkDataCookedIndex& ChunkGroup) {
        Ar << ChunkGroup.Value;
        return Ar;
    }

    friend uint32_t GetTypeHash(const FBulkDataCookedIndex& ChunkGroup) {
        return ChunkGroup.Value;
    }

    static FBulkDataCookedIndex ParseFromPath(const std::string& Path) {
        int ExtensionStartIndex = -1;

        for (int Index = static_cast<int>(Path.size()) - 1; Index >= 0; --Index) {
            if (Path[Index] == '/' || Path[Index] == '\\') {
                return FBulkDataCookedIndex();
            }
            else if (Path[Index] == '.') {
                if (ExtensionStartIndex != -1) {
                    std::string Extension = Path.substr(Index + 1, ExtensionStartIndex - Index - 1);

                    // Check if the extension contains only digits
                    if (std::all_of(Extension.begin(), Extension.end(), ::isdigit)) {
                        ValueType Value = 0;
                        Value = static_cast<ValueType>(std::stoul(Extension));

                        return FBulkDataCookedIndex(Value);
                    }
                    else {
                        return FBulkDataCookedIndex();
                    }
                }
                else {
                    ExtensionStartIndex = Index;
                }
            }
        }

        return FBulkDataCookedIndex();
    }
private:
    ValueType Value = 0;
};