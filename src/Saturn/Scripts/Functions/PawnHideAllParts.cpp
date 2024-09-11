import Saturn.Functions.PawnHideAllParts;

import Saturn.Functions.PlayerGetPawn;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

void FPawnHideAllParts::encode(int pawn) {
	std::string message = std::to_string((int)EOpcodes::PAWNHIDEALLPARTS) + ",,,,," + std::to_string(pawn);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPawnHideAllParts(Pawn);
duk_ret_t FPawnHideAllParts::dukPawnHideAllParts(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 argument!", L"PawnHideAllParts", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t pawn = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));

	if (pawn % FPlayerGetPawn::PAWN_SIG != 0 || pawn == 0) {
		MessageBoxW(nullptr, L"Pawn pointer invalid!", L"PawnHideAllParts", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(pawn / FPlayerGetPawn::PAWN_SIG);
	while (FContext::ResponseWaiting);


	return 0;
}