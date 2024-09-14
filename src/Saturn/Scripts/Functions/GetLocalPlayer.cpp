import Saturn.Functions.GetLocalPlayer;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

int FGetLocalPlayer::PlayerCount = 0;

void FGetLocalPlayer::encode() {
	std::string message = std::to_string((int)EOpcodes::GETLOCALPLAYER) + ",,,,," + std::to_string(++PlayerCount);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

duk_ret_t FGetLocalPlayer::dukGetLocalPlayer(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 0) {
		MessageBoxW(nullptr, L"This function takes 0 arguments!", L"GetLocalPlayer", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode();
	while (FContext::ResponseWaiting);

	duk_push_pointer(ctx, reinterpret_cast<void*>(PlayerCount * PLAYER_SIG));

	return 1;
}