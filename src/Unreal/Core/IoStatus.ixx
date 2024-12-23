module;

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

export FIoStatusBuilder operator<<(const FIoStatus& Status, const std::string& String);