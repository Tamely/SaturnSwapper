import Saturn.Functions.PlayerGetPawn;
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

int FPlayerGetPawn::PlayerPawnCount = 0;

void FPlayerGetPawn::encode(int parent) {
	std::string message = std::to_string((int)EOpcodes::GETPAWNFROMPLAYER) + ",,,,," + std::to_string(++PlayerPawnCount) + ",,,,," + std::to_string(parent);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPlayerPawn UPlayerGetPawn(Player);
duk_ret_t FPlayerGetPawn::dukPlayerGetPawn(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 argument!", L"PlayerGetPawn", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	int64_t player = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	if (player % FGetLocalPlayer::PLAYER_SIG != 0 || player == 0) {
		MessageBoxW(nullptr, L"Player pointer invalid!", L"PlayerGetPawn", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(player / FGetLocalPlayer::PLAYER_SIG);
	while (FContext::ResponseWaiting);

	duk_push_pointer(ctx, reinterpret_cast<void*>(PlayerPawnCount * PAWN_SIG));

	return 1;
}