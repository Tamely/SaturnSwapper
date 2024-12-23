export module Saturn.Structs.IoContainerId;

import Saturn.Readers.FArchive;
import <cstdint>;

export class FIoContainerId {
public:
	inline FIoContainerId() = default;
	inline FIoContainerId(const FIoContainerId& Other) = default;
	inline FIoContainerId(FIoContainerId&& Other) = default;
	inline FIoContainerId& operator=(const FIoContainerId& Other) = default;

	uint64_t Value() const {
		return Id;
	}

	inline bool IsValid() const {
		return Id != InvalidId;
	}

	inline bool operator<(FIoContainerId Other) const {
		return Id < Other.Id;
	}

	inline bool operator==(FIoContainerId Other) const {
		return Id == Other.Id;
	}

	inline bool operator!=(FIoContainerId Other) const {
		return Id != Other.Id;
	}

	inline friend uint32_t GetTypeHash(const FIoContainerId& In) {
		return uint32_t(In.Id);
	}

	friend FArchive& operator<<(FArchive& Ar, FIoContainerId& ContainerId) {
        return Ar << ContainerId.Id;
    };

private:
	inline explicit FIoContainerId(const uint64_t InId)
		: Id(InId) { }

	static constexpr uint64_t InvalidId = uint64_t(-1);

	uint64_t Id = InvalidId;
};