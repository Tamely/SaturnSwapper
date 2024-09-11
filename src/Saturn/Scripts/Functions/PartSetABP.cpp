import Saturn.Functions.PartSetABP;

import Saturn.Functions.PawnGetPart;
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

void FPartSetABP::encode(int part, int mesh) {
	std::string message = std::to_string((int)EOpcodes::PARTSETABP) + ",,,,," + std::to_string(part) + ",,,,," + std::to_string(mesh);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPartSetABP(Part, ABP);
duk_ret_t FPartSetABP::dukPartSetABP(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 2) {
		MessageBoxW(nullptr, L"This function takes 2 arguments!", L"PartSetABP", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t part = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int64_t abp = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 1));

	if (part % FPawnGetPart::COMPONENT_SIG != 0 || part == 0) {
		MessageBoxW(nullptr, L"Part pointer invalid!", L"PartSetABP", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	if (abp % FFindABPByPath::ABP_SIG != 0 || abp == 0) {
		MessageBoxW(nullptr, L"ABP pointer invalid!", L"PartSetABP", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(part / FPawnGetPart::COMPONENT_SIG, abp / FFindABPByPath::ABP_SIG);
	while (FContext::ResponseWaiting);

	return 0;
}