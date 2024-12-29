import Saturn.Core.IoStatus;

#include "Saturn/Log.h"

import <string>;
#include <algorithm>

void Convert(char* OutStr, int32_t OutStrSize, const char* InStr, int32_t InStrSize) {
    if (InStrSize > 0) {
        for (int32_t i = 0; i < InStrSize; ++i) {
            OutStr[i] = static_cast<char>(InStr[i]);
        }
        OutStr[InStrSize] = '\0';
    }
    else {
        OutStr[0] = '\0';
    }
}

const FIoStatus FIoStatus::Ok { EIoErrorCode::Ok, "OK" };
const FIoStatus FIoStatus::Unknown { EIoErrorCode::Unknown, "Unknown Status" };
const FIoStatus FIoStatus::Invalid { EIoErrorCode::InvalidCode, "Invalid Code" };

FIoStatus::FIoStatus() {}
FIoStatus::~FIoStatus() {}

FIoStatus::FIoStatus(EIoErrorCode Code) : ErrorCode(Code) {
    ErrorMessage[0] = '\0';
}

FIoStatus::FIoStatus(EIoErrorCode Code, const std::string& InErrorMessage) : ErrorCode(Code) {
    const int32_t ErrorMessageLength = std::min(MaxErrorMessageLength - 1, static_cast<int>(InErrorMessage.size()));
    Convert(ErrorMessage, ErrorMessageLength, InErrorMessage.c_str(), ErrorMessageLength);
    ErrorMessage[ErrorMessageLength] = '\0';
}

FIoStatus& FIoStatus::operator=(const FIoStatus& Other) {
    ErrorCode = Other.ErrorCode;
    memcpy(ErrorMessage, Other.ErrorMessage, MaxErrorMessageLength);

    return *this;
}

FIoStatus& FIoStatus::operator=(const EIoErrorCode InErrorCode) {
    ErrorCode = InErrorCode;
    ErrorMessage[0] = '\0';

    return *this;
}

bool FIoStatus::operator==(const FIoStatus& Other) const {
    return ErrorCode == Other.ErrorCode &&
        strcmp(ErrorMessage, Other.ErrorMessage) == 0;
}

std::string FIoStatus::ToString() const {
    return std::string(ErrorMessage) + "(" + GetIoErrorText(ErrorCode) + ")";
}

FIoStatusBuilder::FIoStatusBuilder(EIoErrorCode InStatusCode) : StatusCode(InStatusCode) {}

FIoStatusBuilder::FIoStatusBuilder(const FIoStatus& InStatus, const std::string& String) : StatusCode(InStatus.ErrorCode) {
    Message += String;
}

FIoStatusBuilder::~FIoStatusBuilder() {}

FIoStatusBuilder::operator FIoStatus() {
    return FIoStatus(StatusCode, Message);
}

FIoStatusBuilder& FIoStatusBuilder::operator<<(const std::string& String) {
    Message += String;

    return *this;
}

FIoStatusBuilder operator<<(const FIoStatus& Status, const std::string& String) {
    return FIoStatusBuilder(Status, String);
}