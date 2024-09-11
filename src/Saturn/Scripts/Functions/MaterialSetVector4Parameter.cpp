import Saturn.Functions.MaterialSetVector4Parameter;
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

void FMaterialSetVector4Parameter::encode(int parent, const std::string& paramName, double r, double g, double b, double a) {
	std::string message = std::to_string((int)EOpcodes::MATERIALSETVEC4PARAMETER) + ",,,,," + std::to_string(parent) + ",,,,," + paramName + ",,,,," + std::to_string(r) + ",,,,," + std::to_string(g) + ",,,,," + std::to_string(b) + ",,,,," + std::to_string(a);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UMaterialSetVector4Parameter(Material, "ParameterName", R.f, G.f, B.f, A.f);
duk_ret_t FMaterialSetVector4Parameter::dukMaterialSetVector4Parameter(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 6) {
		MessageBoxW(nullptr, L"This function takes 6 arguments!", L"MaterialSetVector4ParameterValue", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	int64_t material = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	if (material % FCreateDynamicMaterialInstance::MATERIAL_INSTANCE_SIG != 0 || material == 0) {
		MessageBoxW(nullptr, L"Material pointer invalid!", L"MaterialSetVector4ParameterValue", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string parameter = duk_get_string(ctx, 1);

	double r = duk_get_number(ctx, 2);
	double g = duk_get_number(ctx, 3);
	double b = duk_get_number(ctx, 4);
	double a = duk_get_number(ctx, 5);

	FContext::ResponseWaiting = true;
	encode(material / FCreateDynamicMaterialInstance::MATERIAL_INSTANCE_SIG, parameter, r, g, b, a);
	while (FContext::ResponseWaiting);

	return 0;
}