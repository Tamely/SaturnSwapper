export module Saturn.Functions.ClearMaps;

import Saturn.Context;

import Saturn.Functions.FindABPByPath;
import Saturn.Functions.FindMeshByPath;
import Saturn.Functions.GetLocalPlayer;
import Saturn.Functions.PawnGetPart;
import Saturn.Functions.PlayerGetPawn;
import Saturn.Functions.FindTextureByURL;
import Saturn.Functions.CreateDynamicMaterialInstance;

import <duktape/duktape.h>;
import <string>;
import <Windows.h>;
import Saturn.Functions.FindPartByPath;

export struct FClearMaps {
	static void encode();
	// UClearMaps();
	static duk_ret_t dukClearMaps(duk_context* ctx) {
		int ArgsLength = duk_get_top(ctx);
		if (ArgsLength != 0) {
			MessageBoxW(nullptr, L"This function doesn't take any arguments!", L"ClearMaps", NULL);
			return DUK_RET_TYPE_ERROR;
		}

		FFindABPByPath::ABPCount = 0;
		FFindMeshByPath::MeshCount = 0;
		FFindPartByPath::PartCount = 0;
		FGetLocalPlayer::PlayerCount = 0;
		FPawnGetPart::ComponentCount = 0;
		FPlayerGetPawn::PlayerPawnCount = 0;
		FFindTextureByURL::TextureCount = 0;
		FCreateDynamicMaterialInstance::MaterialInstanceCount = 0;

		FContext::ResponseWaiting = true;
		encode();
		while (FContext::ResponseWaiting);

		return 0;
	}
};