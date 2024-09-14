export module Saturn.Functions.PawnSetBodyType;

import <duktape/duktape.h>;
import <string>;

export struct FPawnSetBodyType {
	static void encode(int pawn, int bodyType);
	/*
	enum class EFortCustomBodyType : uint8 {
		NONE = 0,
		Small = 1,
		Medium = 2,
		MediumAndSmall = 3,
		Large = 4,
		LargeAndSmall = 5,
		LargeAndMedium = 6,
		All = 7,
		Deprecated = 8,
		EFortCustomBodyType_MAX = 9
	};
	*/
	// UPawnSetBodyType(Pawn, BodyType);
	static duk_ret_t dukPawnSetBodyType(duk_context* ctx);
};