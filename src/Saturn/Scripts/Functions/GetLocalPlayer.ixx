export module Saturn.Functions.GetLocalPlayer;

import <duktape/duktape.h>;

export struct FGetLocalPlayer {
	const static int PLAYER_SIG = 0x1337;
	static int PlayerCount;

	static void encode();
	static duk_ret_t dukGetLocalPlayer(duk_context* ctx);
};