export module Saturn.Functions.MaterialSetVector4Parameter;

import <duktape/duktape.h>;
import <string>;

export struct FMaterialSetVector4Parameter {
	static void encode(int parent, const std::string& paramName, double r, double g, double b, double a);
	// UMaterialSetVector4Parameter(Material, "ParameterName", R.f, G.f, B.f, A.f);
	static duk_ret_t dukMaterialSetVector4Parameter(duk_context* ctx);
};