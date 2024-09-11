import Saturn.Functions.MaterialSetTextureParameterValue;
import Saturn.Functions.CreateDynamicMaterialInstance;
import Saturn.Functions.FindTextureByURL;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

void FMaterialSetTextureParameter::encode(int parent, const std::string& paramName, int tex) {
	std::string message = std::to_string((int)EOpcodes::MATERIALSETTEXTUREPARAMETER) + ",,,,," + std::to_string(parent) + ",,,,," + paramName + ",,,,," + std::to_string(tex);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPlayerPawn UMaterialSetTextureParameter(Material, "ParameterName", Texture);
duk_ret_t FMaterialSetTextureParameter::dukMaterialSetTextureParameter(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 3) {
		MessageBoxW(nullptr, L"This function takes 3 arguments!", L"MaterialSetTextureParameterValue", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	int64_t material = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	if (material % FCreateDynamicMaterialInstance::MATERIAL_INSTANCE_SIG != 0 || material == 0) {
		MessageBoxW(nullptr, L"Material pointer invalid!", L"MaterialSetTextureParameterValue", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string parameter = duk_get_string(ctx, 1);

	int64_t texture = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 2));
	if (texture % FFindTextureByURL::TEXTURE_SIG != 0 || texture == 0) {
		MessageBoxW(nullptr, L"Texture pointer invalid!", L"MaterialSetTextureParameterValue", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(material / FCreateDynamicMaterialInstance::MATERIAL_INSTANCE_SIG, parameter, texture / FFindTextureByURL::TEXTURE_SIG);
	while (FContext::ResponseWaiting);

	return 0;
}