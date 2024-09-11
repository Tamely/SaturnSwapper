export module Saturn.Functions.PawnStopMontage;

import <duktape/duktape.h>;
import <string>;

export struct FPawnStopMontage {
	static void encode(int pawn, int montage, float blendOutTime);
	// UPawnStopMontage(Pawn, Montage, BlendOutTime.f);
	static duk_ret_t dukPawnStopMontage(duk_context* ctx);
};