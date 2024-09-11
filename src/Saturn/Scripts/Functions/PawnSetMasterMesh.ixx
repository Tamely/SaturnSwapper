export module Saturn.Functions.PawnSetMasterMesh;

import <duktape/duktape.h>;
import <string>;

export struct FPawnSetMasterMesh {
	static void encode(int pawn, int mesh);
	// UPawnSetMasterMesh(Pawn, Mesh);
	static duk_ret_t dukPawnSetMasterMesh(duk_context* ctx);
};