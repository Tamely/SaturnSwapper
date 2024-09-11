export module Saturn.Functions.MaterialSetFloatParameter;

import <duktape/duktape.h>;
import <string>;

export struct FMaterialSetFloatParameter {
	static void encode(int parent, const std::string& paramName, double param);
	// UMaterialSetFloatParameter(Material, "ParameterName", Param.f);
	static duk_ret_t dukMaterialSetFloatParameter(duk_context* ctx);
};