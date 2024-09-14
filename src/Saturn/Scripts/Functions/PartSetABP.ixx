export module Saturn.Functions.PartSetABP;

import <duktape/duktape.h>;
import <string>;

export struct FPartSetABP {
	static void encode(int part, int abp);
	// UPartSetABP(Part, ABP);
	static duk_ret_t dukPartSetABP(duk_context* ctx);
};