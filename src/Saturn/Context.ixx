module;

#include "Saturn/Defines.h"

export module Saturn.Context;

import Saturn.CosmeticState;
import Saturn.Items.PluginModel;
import Saturn.Items.LoadoutModel;
import Saturn.Unreal.IoStoreWriter;
import Saturn.Structs.FileModification;

import <string>;
import <cstdint>;
import <memory>;
import <vector>;
import <AppCore/AppCore.h>;
import <duktape/duktape.h>;

export class FContext {
public:
	static std::string VERSION;

	static int Tab;

	static std::shared_ptr<IoStoreWriter> Writer;
	static std::shared_ptr<IoStoreWriter> SecondWriter;

	static FLoadout	Loadout;
	static std::vector<FLoadout> Loadouts;
	static ECosmeticState CosmeticState;

	static uint8_t* SearchArray;
	static uint8_t* ReplaceArray;

	static std::vector<FFileModification> FileModifications;

	static std::string Channel;
	static std::string Variant;

	static bool Paid;

	static duk_context* DukContext;
	static bool ResponseWaiting;
	static bool HasInitializedCurl;

	static FPluginModel SelectedPlugin;
};