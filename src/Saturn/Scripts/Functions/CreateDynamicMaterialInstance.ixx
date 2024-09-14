export module Saturn.Functions.CreateDynamicMaterialInstance;

import <duktape/duktape.h>;
import <string>;

export struct FCreateDynamicMaterialInstance {
	const static int MATERIAL_INSTANCE_SIG = 0xDEAD;
	static int MaterialInstanceCount;

	static void encode(const std::string& parent);
	// UDynamicMaterialInstance UCreateDynamicMaterialInstance("Parent");
	static duk_ret_t dukCreateDynamicMaterialInstance(duk_context* ctx);
};