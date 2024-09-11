import Saturn.Functions.PawnAddPart;

import Saturn.Functions.FindPartByPath;
import Saturn.Functions.PlayerGetPawn;
import Saturn.Functions.PawnGetPart;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

void FPawnAddPart::encode(int pawn, int part) {
	std::string message = std::to_string((int)EOpcodes::PAWNADDPART) + ",,,,," + std::to_string(++FPawnGetPart::ComponentCount) + ",,,,," + std::to_string(pawn) + ",,,,," + std::to_string(part);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPartComponent UPawnAddPart(Pawn, Part);
duk_ret_t FPawnAddPart::dukPawnAddPart(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 2) {
		MessageBoxW(nullptr, L"This function takes 2 arguments!", L"PawnAddPart", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t pawn = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int64_t part = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 1));

	if (pawn % FPlayerGetPawn::PAWN_SIG != 0 || pawn == 0) {
		MessageBoxW(nullptr, L"Pawn pointer invalid!", L"PawnAddPart", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	if (part % FFindPartByPath::PART_SIG != 0 || part == 0) {
		MessageBoxW(nullptr, L"Part pointer invalid!", L"PawnAddPart", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(pawn / FPlayerGetPawn::PAWN_SIG, part / FFindPartByPath::PART_SIG);
	while (FContext::ResponseWaiting);

	duk_push_pointer(ctx, reinterpret_cast<void*>(FPawnGetPart::ComponentCount * FPawnGetPart::COMPONENT_SIG));

	return 1;
}