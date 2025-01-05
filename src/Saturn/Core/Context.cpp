#include "Saturn/Defines.h"

import Saturn.Context;

import <AppCore/AppCore.h>;
import <duktape/duktape.h>;

int FContext::Tab;

FLoadout FContext::Loadout;
std::vector<FLoadout> FContext::Loadouts = {};
ECosmeticState FContext::CosmeticState = ECosmeticState::None;

uint8_t* FContext::SearchArray = nullptr;
uint8_t* FContext::ReplaceArray = nullptr;

bool FContext::Paid = false;

std::string FContext::VERSION = "3.0.0";

duk_context* FContext::DukContext = nullptr;
bool FContext::ResponseWaiting = false;
bool FContext::HasInitializedCurl = false;

FPluginModel FContext::SelectedPlugin;