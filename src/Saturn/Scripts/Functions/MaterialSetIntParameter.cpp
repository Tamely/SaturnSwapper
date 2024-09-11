import Saturn.Functions.MaterialSetIntParameter;
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

void FMaterialSetIntParameter::encode(int parent, const std::string& paramName, int param) {
	std::string message = std::to_string((int)EOpcodes::MATERIALSETINTPARAMETER) + ",,,,," + std::to_string(parent) + ",,,,," + paramName + ",,,,," + std::to_string(param);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// MaterialSetIntParameter(Material, "ParameterName", Param);
duk_ret_t FMaterialSetIntParameter::dukMaterialSetIntParameter(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 3) {
		MessageBoxW(nullptr, L"This function takes 3 arguments!", L"MaterialSetIntParameter", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	int64_t material = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	if (material % FCreateDynamicMaterialInstance::MATERIAL_INSTANCE_SIG != 0 || material == 0) {
		MessageBoxW(nullptr, L"Material pointer invalid!", L"MaterialSetIntParameter", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string parameter = duk_get_string(ctx, 1);

	int param = duk_get_int(ctx, 2);

	FContext::ResponseWaiting = true;
	encode(material / FCreateDynamicMaterialInstance::MATERIAL_INSTANCE_SIG, parameter, param);
	while (FContext::ResponseWaiting);

	return 0;
}