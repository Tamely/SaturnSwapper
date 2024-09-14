import Saturn.Functions.ClearMaps;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

void FClearMaps::encode() {
	std::string message = std::to_string((int)EOpcodes::CLEARMAPS);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}