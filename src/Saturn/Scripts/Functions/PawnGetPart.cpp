import Saturn.Functions.PawnGetPart;

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

int FPawnGetPart::ComponentCount = 0;

void FPawnGetPart::encode(int pawn, int type) {
	std::string message = std::to_string((int)EOpcodes::PAWNGETPART) + ",,,,," + std::to_string(++ComponentCount) + ",,,,," + std::to_string(pawn) + ",,,,," + std::to_string(type);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPartComponent UPawnGetPart(Pawn, EFortCustomPartType);
duk_ret_t FPawnGetPart::dukPawnGetPart(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 2) {
		MessageBoxW(nullptr, L"This function takes 2 arguments!", L"PawnGetPart", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t pawn = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int idx = duk_get_int(ctx, 1);

	if (pawn % FPlayerGetPawn::PAWN_SIG != 0 || pawn == 0) {
		MessageBoxW(nullptr, L"Pawn pointer invalid!", L"PawnGetPart", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(pawn / FPlayerGetPawn::PAWN_SIG, idx);
	while (FContext::ResponseWaiting);

	duk_push_pointer(ctx, reinterpret_cast<void*>(ComponentCount * COMPONENT_SIG));

	return 1;
}