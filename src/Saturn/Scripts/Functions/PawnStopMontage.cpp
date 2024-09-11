import Saturn.Functions.PawnStopMontage;

import Saturn.Functions.FindMontageByPath;
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

void FPawnStopMontage::encode(int pawn, int montage, float blendOutTime) {
	std::string message = std::to_string((int)EOpcodes::PAWNSTOPMONTAGE) + ",,,,," + std::to_string(pawn) + ",,,,," + std::to_string(montage) + ",,,,," + std::to_string(blendOutTime);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPawnStopMontage(Pawn, Montage, BlendOutTime.f);
duk_ret_t FPawnStopMontage::dukPawnStopMontage(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 3) {
		MessageBoxW(nullptr, L"This function takes 3 arguments!", L"PawnStopMontage", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	int64_t pawn = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int64_t montage = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 1));
	float blendOutTime = duk_get_number(ctx, 2);

	if (pawn % FPlayerGetPawn::PAWN_SIG != 0 || pawn == 0) {
		MessageBoxW(nullptr, L"Pawn pointer invalid!", L"PawnStopMontage", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	if (montage % FFindMontageByPath::MONTAGE_SIG != 0 || montage == 0) {
		MessageBoxW(nullptr, L"Montage pointer invalid!", L"PawnStopMontage", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(pawn / FPlayerGetPawn::PAWN_SIG, montage / FFindMontageByPath::MONTAGE_SIG, blendOutTime);
	while (FContext::ResponseWaiting);

	return 0;
}