module;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

export module Saturn.Core.IoStatus;

export enum class EIoErrorCode {
    Ok,
    Unknown,
    InvalidCode,
    Cancelled,
    FileOpenFailed,
    FileNotOpen,
    ReadError,
    WriteError,
    NotFound,
    CorruptToc,
    UnknownChunkID,
    InvalidParameter,
    SignatureError,
    InvalidEncryptionKey,
    CompressionError,
    PendingFork,
    PendingEncryptionKey
};

export inline const std::string GetIoErrorText(EIoErrorCode ErrorCode) {
    switch (ErrorCode) {
        case EIoErrorCode::Ok: return "Ok";
        case EIoErrorCode::Unknown: return "Unknown";
        case EIoErrorCode::InvalidCode: return "InvalidCode";
        case EIoErrorCode::Cancelled: return "Cancelled";
        case EIoErrorCode::FileOpenFailed: return "FileOpenFailed";
        case EIoErrorCode::FileNotOpen: return "FileNotOpen";
        case EIoErrorCode::ReadError: return "ReadError";
        case EIoErrorCode::WriteError: return "WriteError";
        case EIoErrorCode::NotFound: return "NotFound";
        case EIoErrorCode::CorruptToc: return "CorruptToc";
        case EIoErrorCode::UnknownChunkID: return "UnknownChunkID";
        case EIoErrorCode::InvalidParameter: return "InvalidParameter";
        case EIoErrorCode::SignatureError: return "SignatureError";
        case EIoErrorCode::InvalidEncryptionKey: return "InvalidEncryptionKey";
        case EIoErrorCode::CompressionError: return "CompressionError";
        case EIoErrorCode::PendingFork: return "PendingFork";
        case EIoErrorCode::PendingEncryptionKey: return "PendingEncryptionKey";
        default: return "Unknown";
    }
}

export class FIoStatus {
public:
    FIoStatus();
    ~FIoStatus();

    FIoStatus(EIoErrorCode Code, const std::string& InErrorMessage);
    FIoStatus(EIoErrorCode Code);
    FIoStatus& operator=(const FIoStatus& Other);
    FIoStatus& operator=(const EIoErrorCode InErrorCode);

    bool operator==(const FIoStatus& Other) const;
    bool operator==(const FIoStatus& Other) { return !operator==(Other); }

    inline bool IsOk() const { return ErrorCode == EIoErrorCode::Ok; }
    inline bool IsCompleted() const { return ErrorCode != EIoErrorCode::Unknown; }
    inline EIoErrorCode GetErrorCode() const { return ErrorCode; }
    std::string ToString() const;

    static const FIoStatus Ok;
    static const FIoStatus Unknown;
    static const FIoStatus Invalid;
private:
    static constexpr int32_t MaxErrorMessageLength = 128;
    using FErrorMessage = char[MaxErrorMessageLength];

    EIoErrorCode ErrorCode = EIoErrorCode::Ok;
    FErrorMessage ErrorMessage;

    friend class FIoStatusBuilder;
};

export class FIoStatusBuilder {
    EIoErrorCode StatusCode;
    std::string Message;
public:
    explicit FIoStatusBuilder(EIoErrorCode StatusCode);
    FIoStatusBuilder(const FIoStatus& InStatus, const std::string& String);
    ~FIoStatusBuilder();

    operator FIoStatus();

    FIoStatusBuilder& operator<<(const std::string& String);
};

export template<typename T>
class TIoStatusOr {
    template<typename U> friend class TIoStatusOr;

public:
    TIoStatusOr() : StatusValue(FIoStatus::Unknown) {}
    TIoStatusOr(const TIoStatusOr& Other);
    TIoStatusOr(TIoStatusOr&& Other);

    TIoStatusOr(FIoStatus InStatus);
    TIoStatusOr(const T& InValue);
    TIoStatusOr(T&& InValue);

    ~TIoStatusOr();

    template <typename... ArgTypes>
    explicit TIoStatusOr(ArgTypes&&... Args);

    template<typename U>
    TIoStatusOr(const TIoStatusOr<U>& Other);

    TIoStatusOr<T>& operator=(const TIoStatusOr<T>& Other);
    TIoStatusOr<T>& operator=(TIoStatusOr<T>&& Other);
    TIoStatusOr<T>& operator=(const FIoStatus& OtherStatus);
    TIoStatusOr<T>& operator=(const T& OtherValue);
    TIoStatusOr<T>& operator=(T&& OtherValue);

    template<typename U>
    TIoStatusOr<T>& operator=(const TIoStatusOr<U>& Other);

    const FIoStatus& Status() const;
    bool IsOk() const;

    const T& ValueOrDie();
    T ConsumeValueOrDie();

    void Reset();
private:
    FIoStatus StatusValue;
    T Value;
};

void StatusOrCrash(const FIoStatus& Status) {
    LOG_CRITICAL("I/O Error '{0}'", Status.ToString());
}

template <typename T>
void TIoStatusOr<T>::Reset() {
    EIoErrorCode ErrorCode = StatusValue.GetErrorCode();
    StatusValue = EIoErrorCode::Unknown;

    if (ErrorCode == EIoErrorCode::Ok) {
        ((T*)&Value)->~T();
    }
}

template <typename T>
const T& TIoStatusOr<T>::ValueOrDie() {
    if (!StatusValue.IsOk()) {
        StatusOrCrash(StatusValue);
    }

    return Value;
}

template<typename T>
T TIoStatusOr<T>::ConsumeValueOrDie() {
    if (!StatusValue.IsOk()) {
        StatusOrCrash(StatusValue);
    }

    StatusValue = FIoStatus::Unknown;

    return std::move(Value);
}

template <typename T>
TIoStatusOr<T>::TIoStatusOr(const TIoStatusOr& Other) {
    StatusValue = Other.StatusValue;
    if (StatusValue.IsOk()) {
        new(&Value) T(*(const T*)&Other.Value);
    }
}

template <typename T>
TIoStatusOr<T>::TIoStatusOr(TIoStatusOr&& Other) {
    StatusValue = Other.StatusValue;
    if (StatusValue.IsOk()) {
        new(&Value) T(std::move(*(T*)&Other.Value));
        Other.StatusValue = EIoErrorCode::Unknown;
    }
}

template <typename T>
TIoStatusOr<T>::TIoStatusOr(FIoStatus InStatus) {
    StatusValue = InStatus;
}

template <typename T>
TIoStatusOr<T>::TIoStatusOr(const T& InValue) {
    StatusValue = FIoStatus::Ok;
    new(&Value) T(InValue);
}

template <typename T>
TIoStatusOr<T>::TIoStatusOr(T&& InValue) {
    StatusValue = FIoStatus::Ok;
    new(&Value) T(std::move(InValue));
}

template <typename T>
template <typename... ArgTypes>
TIoStatusOr<T>::TIoStatusOr(ArgTypes&&... Args) {
    StatusValue = FIoStatus::Ok;
    new(&Value) T(std::forward<ArgTypes>(Args)...);
}

template<typename T>
TIoStatusOr<T>::~TIoStatusOr() {
    Reset();
}

template<typename T>
bool TIoStatusOr<T>::IsOk() const {
    return StatusValue.IsOk();
}

template<typename T>
const FIoStatus& TIoStatusOr<T>::Status() const {
    return StatusValue;
}

template<typename T>
TIoStatusOr<T>&
TIoStatusOr<T>::operator=(const TIoStatusOr<T>& Other) {
    if (&Other != this) {
        Reset();

        if (Other.StatusValue.IsOk()) {
            new(&Value) T(*(const T*)&Other.Value);
            StatusValue = EIoErrorCode::Ok;
        }
        else {
            StatusValue = Other.StatusValue;
        }
    }

    return *this;
}

template<typename T>
TIoStatusOr<T>&
TIoStatusOr<T>::operator=(TIoStatusOr<T>&& Other) {
    if (&Other != this) {
        Reset();

        if (Other.StatusValue.IsOk()) {
            new(&Value) T(std::move(*(T*)&Other.Value));
            Other.StatusValue = EIoErrorCode::Unknown;
            StatusValue = EIoErrorCode::Ok;
        }
        else {
            StatusValue = Other.StatusValue;
        }
    }

    return *this;
}

template<typename T>
TIoStatusOr<T>&
TIoStatusOr<T>::operator=(const FIoStatus& OtherStatus) {
    Reset();
    StatusValue = OtherStatus;

    return *this;
}

template<typename T>
TIoStatusOr<T>&
TIoStatusOr<T>::operator=(const T& OtherValue) {
    if (&OtherValue != (T*)&Value)
    {
        Reset();

        new(&Value) T(OtherValue);
        StatusValue = EIoErrorCode::Ok;
    }

    return *this;
}

template<typename T>
TIoStatusOr<T>&
TIoStatusOr<T>::operator=(T&& OtherValue) {
    if (&OtherValue != (T*)&Value) {
        Reset();

        new(&Value) T(std::move(OtherValue));
        StatusValue = EIoErrorCode::Ok;
    }

    return *this;
}

template<typename T>
template<typename U>
TIoStatusOr<T>::TIoStatusOr(const TIoStatusOr<U>& Other)
    : StatusValue(Other.StatusValue) {
    if (StatusValue.IsOk()) {
        new(&Value) T(*(const U*)&Other.Value);
    }
}

template<typename T>
template<typename U>
TIoStatusOr<T>& TIoStatusOr<T>::operator=(const TIoStatusOr<U>& Other) {
    Reset();

    if (Other.StatusValue.IsOk()) {
        new(&Value) T(*(const U*)&Other.Value);
        StatusValue = EIoErrorCode::Ok;
    }
    else {
        StatusValue = Other.StatusValue;
    }

    return *this;
}

export FIoStatusBuilder operator<<(const FIoStatus& Status, const std::string& String);