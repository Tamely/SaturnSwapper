export module Saturn.Functions.MaterialSetIntParameter;

import <duktape/duktape.h>;
import <string>;

export struct FMaterialSetIntParameter {
	static void encode(int parent, const std::string& paramName, int param);
	// UMaterialSetTextureParameter(Material, "ParameterName", Texture);
	static duk_ret_t dukMaterialSetIntParameter(duk_context* ctx);
};