export module Saturn.Functions.PlayerGetPawn;

import <duktape/duktape.h>;
import <string>;

export struct FPlayerGetPawn {
	const static int PAWN_SIG = 0xDEAD;
	static int PlayerPawnCount;

	static void encode(int parent);
	// UPlayerPawn UPlayerGetPawn(Player);
	static duk_ret_t dukPlayerGetPawn(duk_context* ctx);
};