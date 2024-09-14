export module Saturn.Functions.MaterialSetBoolParameter;

import <duktape/duktape.h>;
import <string>;

export struct FMaterialSetBoolParameter {
	static void encode(int parent, const std::string& paramName, bool param);
	// UMaterialSetBoolParameter(Material, "ParameterName", Texture);
	static duk_ret_t dukMaterialSetBoolParameter(duk_context* ctx);
};