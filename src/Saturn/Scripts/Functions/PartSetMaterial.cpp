import Saturn.Functions.PartSetMaterial;

import Saturn.Functions.CreateDynamicMaterialInstance;
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

void FPartSetMaterial::encode(int part, int material, int idx) {
	std::string message = std::to_string((int)EOpcodes::PARTSETMATERIAL) + ",,,,," + std::to_string(part) + ",,,,," + std::to_string(material) + ",,,,," + std::to_string(idx);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPartSetMaterial(Part, Material, Index);
duk_ret_t FPartSetMaterial::dukPartSetMaterial(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 3) {
		MessageBoxW(nullptr, L"This function takes 3 arguments!", L"PartSetMaterial", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t part = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int64_t material = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 1));
	int idx = duk_get_int(ctx, 2);

	if (part % FPawnGetPart::COMPONENT_SIG != 0) {
		MessageBoxW(nullptr, L"Part pointer invalid!", L"PartSetMaterial", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	if (material % FCreateDynamicMaterialInstance::MATERIAL_INSTANCE_SIG != 0) {
		MessageBoxW(nullptr, L"Material pointer invalid!", L"PartSetMaterial", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(part / FPawnGetPart::COMPONENT_SIG, material / FCreateDynamicMaterialInstance::MATERIAL_INSTANCE_SIG, idx);
	while (FContext::ResponseWaiting);

	return 0;
}