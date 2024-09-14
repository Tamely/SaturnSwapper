export module Saturn.Functions.PartSetMesh;

import <duktape/duktape.h>;
import <string>;

export struct FPartSetMesh {
	static void encode(int part, int mesh);
	// UPartSetMesh(Part, Mesh);
	static duk_ret_t dukPartSetMesh(duk_context* ctx);
};