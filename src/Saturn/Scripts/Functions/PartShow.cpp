import Saturn.Functions.PartShow;

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

void FPartShow::encode(int part) {
	std::string message = std::to_string((int)EOpcodes::PARTSHOW) + ",,,,," + std::to_string(part);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPartShow(Part);
duk_ret_t FPartShow::dukPartShow(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 argument!", L"PartShow", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t part = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));

	if (part % FPawnGetPart::COMPONENT_SIG != 0 || part == 0) {
		MessageBoxW(nullptr, L"Part pointer invalid!", L"PartSetABP", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(part / FPawnGetPart::COMPONENT_SIG);
	while (FContext::ResponseWaiting);

	return 0;
}