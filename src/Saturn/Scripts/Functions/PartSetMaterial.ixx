export module Saturn.Functions.PartSetMaterial;

import <duktape/duktape.h>;
import <string>;

export struct FPartSetMaterial {
	static void encode(int part, int material, int idx);
	// UPartSetMaterial(Part, Material, Index);
	static duk_ret_t dukPartSetMaterial(duk_context* ctx);
};