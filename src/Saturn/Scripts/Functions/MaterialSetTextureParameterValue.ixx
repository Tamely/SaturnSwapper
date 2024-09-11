export module Saturn.Functions.MaterialSetTextureParameterValue;

import <duktape/duktape.h>;
import <string>;

export struct FMaterialSetTextureParameter {
	static void encode(int parent, const std::string& paramName, int tex);
	// UPlayerPawn UMaterialSetTextureParameter(Material, "ParameterName", Texture);
	static duk_ret_t dukMaterialSetTextureParameter(duk_context* ctx);
};