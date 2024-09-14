import Saturn.Functions.PawnPlayMontage;

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

void FPawnPlayMontage::encode(int pawn, int montage, float playRate, float timeToStartAt, bool stopAllMontages) {
	std::string message = std::to_string((int)EOpcodes::PAWNPLAYMONTAGE) + ",,,,," + std::to_string(pawn) + ",,,,," + std::to_string(montage) + ",,,,," + std::to_string(playRate) + ",,,,," + std::to_string(timeToStartAt) + ",,,,," + std::to_string((int)stopAllMontages);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPawnPlayMontage(Pawn, Montage, PlayRate.f, TimeToStartAt.f, StopAllMontages);
duk_ret_t FPawnPlayMontage::dukPawnPlayMontage(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 5) {
		MessageBoxW(nullptr, L"This function takes 5 arguments!", L"PawnPlayMontage", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	int64_t pawn = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int64_t montage = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 1));
	float playRate = duk_get_number(ctx, 2);
	float timeToStartAt = duk_get_number(ctx, 3);
	bool stopAllMontages = duk_get_boolean(ctx, 4);

	if (pawn % FPlayerGetPawn::PAWN_SIG != 0 || pawn == 0) {
		MessageBoxW(nullptr, L"Pawn pointer invalid!", L"PawnPlayMontage", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	if (montage % FFindMontageByPath::MONTAGE_SIG != 0 || montage == 0) {
		MessageBoxW(nullptr, L"Montage pointer invalid!", L"PawnPlayMontage", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(pawn / FPlayerGetPawn::PAWN_SIG, montage / FFindMontageByPath::MONTAGE_SIG, playRate, timeToStartAt, stopAllMontages);
	while (FContext::ResponseWaiting);

	return 0;
}