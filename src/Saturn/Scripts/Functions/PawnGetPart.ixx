export module Saturn.Functions.PawnGetPart;

import <duktape/duktape.h>;
import <string>;

export struct FPawnGetPart {
	const static int COMPONENT_SIG = 0xBBBB;
	static int ComponentCount;

	static void encode(int pawn, int type);
	// UPartComponent UPawnGetPart(Pawn, EFortCustomPartType);
	/*
		// Enum FortniteGame.EFortCustomPartType
		enum class EFortCustomPartType : uint8 {
			Head = 0,
			Body = 1,
			Hat = 2,
			Backpack = 3,
			MiscOrTail = 4,
			Face = 5,
			Gameplay = 6,
			NumTypes = 7,
			EFortCustomPartType_MAX = 8
		};
	*/
	static duk_ret_t dukPawnGetPart(duk_context* ctx);
};