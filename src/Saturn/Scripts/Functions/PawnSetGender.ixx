export module Saturn.Functions.PawnSetGender;

import <duktape/duktape.h>;
import <string>;

export struct FPawnSetGender {
	static void encode(int pawn, int gender);
	/*
	enum class EFortCustomGender : uint8 {
		Invalid = 0,
		Male = 1,
		Female = 2,
		Both = 3,
		EFortCustomGender_MAX = 4
	};
	*/
	// UPawnSetGender(Pawn, Gender);
	static duk_ret_t dukPawnSetGender(duk_context* ctx);
};