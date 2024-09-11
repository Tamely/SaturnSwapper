import Saturn.Functions.CreateDynamicMaterialInstance;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

int FCreateDynamicMaterialInstance::MaterialInstanceCount = 0;

void FCreateDynamicMaterialInstance::encode(const std::string& parent) {
	std::string message = std::to_string((int)EOpcodes::CREATEMATERIALINSTANCE) + ",,,,," + std::to_string(++MaterialInstanceCount) + ",,,,," + parent;
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UDynamicMaterialInstance UCreateDynamicMaterialInstance("Parent");
duk_ret_t FCreateDynamicMaterialInstance::dukCreateDynamicMaterialInstance(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 argument!", L"CreateDynamicMaterialInstance", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string parent = duk_get_string(ctx, 0);

	FContext::ResponseWaiting = true;
	encode(parent);
	while (FContext::ResponseWaiting);

	duk_push_pointer(ctx, reinterpret_cast<void*>(MaterialInstanceCount * MATERIAL_INSTANCE_SIG));

	return 1;
}