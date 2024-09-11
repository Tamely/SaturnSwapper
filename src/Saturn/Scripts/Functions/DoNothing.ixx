export module Saturn.Functions.DoNothing;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <LodePNG/lodepng.h>;

import <string>;
import <vector>;

export struct FDoNothing {
	static void encode() {
		static std::string message = std::to_string((int)EOpcodes::NONE);
		WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);

		FContext::ResponseWaiting = false;
	}
};