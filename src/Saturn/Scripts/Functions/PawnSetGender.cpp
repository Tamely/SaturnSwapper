import Saturn.Functions.PawnSetGender;

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

void FPawnSetGender::encode(int pawn, int gender) {
	std::string message = std::to_string((int)EOpcodes::PAWNSETGENDER) + ",,,,," + std::to_string(pawn) + ",,,,," + std::to_string(gender);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPawnSetGender(Pawn, Gender);
duk_ret_t FPawnSetGender::dukPawnSetGender(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 2) {
		MessageBoxW(nullptr, L"This function takes 2 arguments!", L"PawnSetGender", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t pawn = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int gender = duk_get_int(ctx, 1);

	if (pawn % FPlayerGetPawn::PAWN_SIG != 0 || pawn == 0) {
		MessageBoxW(nullptr, L"Pawn pointer invalid!", L"PawnSetGender", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	FContext::ResponseWaiting = true;
	encode(pawn / FPlayerGetPawn::PAWN_SIG, gender);
	while (FContext::ResponseWaiting);

	return 0;
}