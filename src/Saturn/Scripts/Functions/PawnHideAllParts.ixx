export module Saturn.Functions.PawnHideAllParts;

import <duktape/duktape.h>;
import <string>;

export struct FPawnHideAllParts {
	static void encode(int pawn);
	// UPawnHideAllParts(Pawn);
	static duk_ret_t dukPawnHideAllParts(duk_context* ctx);
};