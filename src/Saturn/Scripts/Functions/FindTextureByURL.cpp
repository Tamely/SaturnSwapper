import Saturn.Functions.FindTextureByURL;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

int FFindTextureByURL::TextureCount = 0;

void FFindTextureByURL::encode(const std::string& URL) {
	std::string message = std::to_string((int)EOpcodes::GETTEXTUREBYURL) + ",,,,," + std::to_string(++TextureCount) + ",,,,," + URL;
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

duk_ret_t FFindTextureByURL::dukFindTextureByURL(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 arguments!", L"FindTextureByURL", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string url = duk_get_string(ctx, 0);

	FContext::ResponseWaiting = true;
	encode(url);
	while (FContext::ResponseWaiting);

	duk_push_pointer(ctx, reinterpret_cast<void*>(TextureCount * TEXTURE_SIG));

	return 1;
}