export module Saturn.Functions.PawnPlayMontage;

import <duktape/duktape.h>;
import <string>;

export struct FPawnPlayMontage {
	static void encode(int pawn, int montage, float playRate, float timeToStartAt, bool stopAllMontages);
	// UPawnPlayMontage(Pawn, Montage, PlayRate.f, TimeToStartAt.f, StopAllMontages);
	static duk_ret_t dukPawnPlayMontage(duk_context* ctx);
};