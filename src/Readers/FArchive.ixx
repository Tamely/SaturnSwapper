export module Saturn.Readers.FArchive;

import Saturn.Core.UObject;

import <string>;
import <vector>;
import <memory>;

enum { INDEX_NONE = -1 };

template <typename T> struct TCanBulkSerialize { enum { Value = false }; };
template<> struct TCanBulkSerialize<unsigned int> { enum { Value = true }; };
template<> struct TCanBulkSerialize<unsigned short> { enum { Value = true }; };
template<> struct TCanBulkSerialize<int> { enum { Value = true }; };

export class FArchive
{
private:

	bool ArIsFilterEditorOnly = false;
	bool bUseUnversionedProperties = false;

public:

	virtual ~FArchive();

	virtual void Serialize(void* V, int64_t Length) { }
	virtual void WriteBuffer(void* V, int64_t Length) { }

	virtual FArchive& operator<<(UObjectPtr& Value)
	{
		return *this;
	}

	inline FArchive& operator<<(UClassPtr& Value)
	{
		return *this << reinterpret_cast<UObjectPtr&>(Value);
	}

	inline FArchive& operator<<(UStructPtr& Value)
	{
		return *this << reinterpret_cast<UObjectPtr&>(Value);
	}

	template<class T1, class T2>
	friend FArchive& operator<<(FArchive& Ar, std::pair<T1, T2>& InPair)
	{
		Ar << InPair.first;
		Ar << InPair.second;

		return Ar;
	}

	template<class T1, class T2>
	friend FArchive& operator>>(FArchive& Ar, std::pair<T1, T2>& InPair) {
		Ar >> InPair.first;
		Ar >> InPair.second;

		return Ar;
	}

	template<typename T>
	friend FArchive& operator<<(FArchive& Ar, std::vector<T>& InArray)
	{
		if constexpr (sizeof(T) == 1 || TCanBulkSerialize<T>::Value)
		{
			return Ar.BulkSerializeArray(InArray);
		}

		int32_t ArrayNum;
		Ar << ArrayNum;

		if (ArrayNum == 0)
		{
			InArray.clear();
			return Ar;
		}

		InArray.resize(ArrayNum);

		for (auto i = 0; i < InArray.size(); i++)
			Ar << InArray[i];

		return Ar;
	}

	template<typename T>
	friend FArchive& operator>>(FArchive& Ar, std::vector<T>& InArray) {
		Ar >> InArray.size();

		for (int i = 0; i < InArray.size(); i++) {
			Ar >> InArray[i];
		}

		return Ar;
	}

	template<typename T>
	__forceinline FArchive& BulkSerializeArray(std::vector<T>& InArray) {
		int32_t ArrayNum;
		*this << ArrayNum;

		if (ArrayNum == 0)
		{
			InArray.clear();
			return *this;
		}

		return BulkSerializeArray(InArray, ArrayNum);
	}

	template<typename T>
	__forceinline FArchive& BulkWriteArray(std::vector<T>& InArray) {
		*this >> InArray.size();

		return BulkSerializeArray(InArray, InArray.size());
	}

	template<typename T>
	FArchive& BulkSerializeArray(std::vector<T>& InArray, int32_t Count) {
		InArray.resize(Count);

		this->Serialize(InArray.data(), Count * sizeof(T));

		return *this;
	}

	template<typename T>
	FArchive& BulkWriteArray(std::vector<T>& InArray, int32_t Count) {
		this->WriteBuffer(InArray.data(), Count * sizeof(T));

		return *this;
	}

	template<typename T>
	__forceinline void BulkSerialize(void* V) // the idea here is to save time by reducing the amount of serialization operations done, but a few conditions have to be met before using this. i would just avoid this for now
	{
		Serialize(V, sizeof(T));
	}


	friend FArchive& operator<<(FArchive& Ar, std::string& InString);
	friend FArchive& operator<<(FArchive& Ar, int32_t& InNum);
	friend FArchive& operator<<(FArchive& Ar, uint32_t& InNum);
	friend FArchive& operator<<(FArchive& Ar, uint64_t& InNum);
	friend FArchive& operator<<(FArchive& Ar, int64_t& InNum);
	friend FArchive& operator<<(FArchive& Ar, uint8_t& InNum);
	friend FArchive& operator<<(FArchive& Ar, int8_t& InNum);
	friend FArchive& operator<<(FArchive& Ar, uint16_t& InNum);
	friend FArchive& operator<<(FArchive& Ar, float& InNum);
	friend FArchive& operator<<(FArchive& Ar, double& InNum);
	friend FArchive& operator<<(FArchive& Ar, int16_t& InNum);
	friend FArchive& operator<<(FArchive& Ar, bool& InBool);

	friend FArchive& operator>>(FArchive& Ar, std::string& InString);
	friend FArchive& operator>>(FArchive& Ar, int32_t InNum);
	friend FArchive& operator>>(FArchive& Ar, uint32_t InNum);
	friend FArchive& operator>>(FArchive& Ar, uint64_t InNum);
	friend FArchive& operator>>(FArchive& Ar, int64_t InNum);
	friend FArchive& operator>>(FArchive& Ar, uint8_t InNum);
	friend FArchive& operator>>(FArchive& Ar, int8_t InNum);
	friend FArchive& operator>>(FArchive& Ar, uint16_t InNum);
	friend FArchive& operator>>(FArchive& Ar, float InNum);
	friend FArchive& operator>>(FArchive& Ar, double InNum);
	friend FArchive& operator>>(FArchive& Ar, int16_t InNum);
	friend FArchive& operator>>(FArchive& Ar, bool InBool);

	virtual void Seek(int64_t InPos) { }

	virtual void* Data() { return nullptr; }

	__forceinline void SeekCur(int64_t InAdvanceCount)
	{
		Seek(Tell() + InAdvanceCount);
	}

	template <typename T>
	__forceinline void SeekCur()
	{
		SeekCur(sizeof(T));
	}

	virtual int64_t Tell()
	{
		return INDEX_NONE;
	}

	virtual int64_t TotalSize()
	{
		return INDEX_NONE;
	}

	__forceinline bool UseUnversionedPropertySerialization()
	{
		return bUseUnversionedProperties;
	}

	__forceinline void SetUnversionedProperties(bool IsUsingUnversionedProperties)
	{
		bUseUnversionedProperties = IsUsingUnversionedProperties;
	}

	__forceinline bool IsFilterEditorOnly() const
	{
		return ArIsFilterEditorOnly;
	}

	__forceinline void SetFilterEditorOnly(bool InFilterEditorOnly)
	{
		ArIsFilterEditorOnly = InFilterEditorOnly;
	}
};

export typedef std::unique_ptr<FArchive> FUniqueAr;
export typedef std::shared_ptr<FArchive> FSharedAr;