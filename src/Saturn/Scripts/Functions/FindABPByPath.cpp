import Saturn.Functions.FindABPByPath;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

int FFindABPByPath::ABPCount = 0;

void FFindABPByPath::encode(const std::string& path) {
	std::string message = std::to_string((int)EOpcodes::GETABPBYPATH) + ",,,,," + std::to_string(++FFindABPByPath::ABPCount) + ",,,,," + path;
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

duk_ret_t FFindABPByPath::dukFindABPByPath(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 arguments!", L"FindABPByPath", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string path = duk_get_string(ctx, 0);

	FContext::ResponseWaiting = true;
	encode(path);
	while (FContext::ResponseWaiting);

	duk_push_pointer(ctx, reinterpret_cast<void*>(FFindABPByPath::ABPCount * FFindABPByPath::ABP_SIG));

	return 1;
}