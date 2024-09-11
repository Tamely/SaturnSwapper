import Saturn.Functions.FindPartByPath;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

int FFindPartByPath::PartCount = 0;

void FFindPartByPath::encode(const std::string& path) {
	std::string message = std::to_string((int)EOpcodes::GETPARTBYPATH) + ",,,,," + std::to_string(++FFindPartByPath::PartCount) + ",,,,," + path;
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

duk_ret_t FFindPartByPath::dukFindPartByPath(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 arguments!", L"FindPartByPath", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string path = duk_get_string(ctx, 0);

	FContext::ResponseWaiting = true;
	encode(path);
	while (FContext::ResponseWaiting);

	duk_push_pointer(ctx, reinterpret_cast<void*>(FFindPartByPath::PartCount * FFindPartByPath::PART_SIG));

	return 1;
}