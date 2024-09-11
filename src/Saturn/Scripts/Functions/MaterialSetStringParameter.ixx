export module Saturn.Functions.MaterialSetStringParameter;

import <duktape/duktape.h>;
import <string>;

export struct FMaterialSetStringParameter {
	static void encode(int parent, const std::string& paramName, const std::string& param);
	// UMaterialSetVector4Parameter(Material, "ParameterName", "Param");
	static duk_ret_t dukMaterialSetStringParameter(duk_context* ctx);
};