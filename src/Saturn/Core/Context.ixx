module;

#include "Saturn/Defines.h"

export module Saturn.Context;

import Saturn.CosmeticState;
import Saturn.Items.PluginModel;
import Saturn.Items.LoadoutModel;
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

	static FLoadout	Loadout;
	static std::vector<FLoadout> Loadouts;
	static ECosmeticState CosmeticState;

	static uint8_t* SearchArray;
	static uint8_t* ReplaceArray;

	static bool Paid;

	static duk_context* DukContext;
	static bool ResponseWaiting;
	static bool HasInitializedCurl;

	static FPluginModel SelectedPlugin;
};