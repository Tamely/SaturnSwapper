export module Saturn.Functions.PawnAddPart;

import <duktape/duktape.h>;
import <string>;

export struct FPawnAddPart {
	static void encode(int pawn, int part);
	// UPartComponent UPawnAddPart(Pawn, Part);
	static duk_ret_t dukPawnAddPart(duk_context* ctx);
};