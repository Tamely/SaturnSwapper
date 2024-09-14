#include <string>

import Saturn.Readers.FArchive;

FArchive::~FArchive() {}

FArchive& operator<<(FArchive& Ar, std::string& InString)
{
	int32_t SaveNum = 0;
	Ar << SaveNum;

	bool bLoadUnicodeChar = SaveNum < 0;
	if (bLoadUnicodeChar) SaveNum = -SaveNum;

	if (!SaveNum) return Ar;

	if (bLoadUnicodeChar)
	{
		auto WStringData = std::make_unique<wchar_t[]>(SaveNum);
		Ar.Serialize(WStringData.get(), SaveNum * sizeof(wchar_t));

		auto Temp = std::wstring(WStringData.get());
		InString.assign(Temp.begin(), Temp.end());
	}
	else
	{
		InString.resize(SaveNum);
		Ar.Serialize(&InString[0], SaveNum);
	}

	return Ar;
}

FArchive& operator<<(FArchive& Ar, int32_t& InNum)
{
	Ar.Serialize(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, uint32_t& InNum)
{
	Ar.Serialize(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, uint64_t& InNum)
{
	Ar.Serialize(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, int64_t& InNum)
{
	Ar.Serialize(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, uint8_t& InByte)
{
	Ar.Serialize(&InByte, sizeof(InByte));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, int8_t& InByte)
{
	Ar.Serialize(&InByte, sizeof(InByte));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, int16_t& InNum)
{
	Ar.Serialize(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, uint16_t& InNum)
{
	Ar.Serialize(&InNum, sizeof(InNum));
	return Ar;
}


FArchive& operator<<(FArchive& Ar, float& InNum)
{
	Ar.Serialize(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, double& InNum)
{
	Ar.Serialize(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator<<(FArchive& Ar, bool& InBool)
{
	uint32_t UBool;
	Ar.Serialize(&UBool, sizeof(UBool));

	InBool = UBool;

	return Ar;
}

FArchive& operator>>(FArchive& Ar, std::string& InString)
{
	Ar >> InString.size();
	Ar.WriteBuffer(&InString[0], InString.size());

	return Ar;
}

FArchive& operator>>(FArchive& Ar, int32_t InNum)
{
	Ar.WriteBuffer(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, uint32_t InNum)
{
	Ar.WriteBuffer(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, uint64_t InNum)
{
	Ar.WriteBuffer(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, int64_t InNum)
{
	Ar.WriteBuffer(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, uint8_t InByte)
{
	Ar.WriteBuffer(&InByte, sizeof(InByte));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, int8_t InByte)
{
	Ar.WriteBuffer(&InByte, sizeof(InByte));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, int16_t InNum)
{
	Ar.WriteBuffer(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, uint16_t InNum)
{
	Ar.WriteBuffer(&InNum, sizeof(InNum));
	return Ar;
}


FArchive& operator>>(FArchive& Ar, float InNum)
{
	Ar.WriteBuffer(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, double InNum)
{
	Ar.WriteBuffer(&InNum, sizeof(InNum));
	return Ar;
}

FArchive& operator>>(FArchive& Ar, bool InBool)
{
	Ar.WriteBuffer(&InBool, sizeof(InBool));
	return Ar;
}