#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

import Saturn.Items.LoadoutModel;

import Saturn.WindowsFunctionLibrary;
import Saturn.FortniteFunctionLibrary;

void FLoadout::WriteToSaveGame(const FLoadout& Loadout) {

	FortniteFunctionLibrary::KillEpicProcesses();
	Sleep(1000);

	FortniteFunctionLibrary::PatchEpicGames();

	// Add a check here for if this worked later please and thank you
	FortniteFunctionLibrary::PatchFortnite(Loadout);


	/*
	* 
	* Maybe change back later
	* 
	static int rvn = 1;

	if (Loadout.Skin.Id.empty() || Loadout.Skin.PackagePath.empty()) {
		return;
	}

	if (rvn == 9) {
		rvn = 0;
	}

	WindowsFunctionLibrary::StringToImage(
		// Path
		WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath()) 
		+ "loadout.png", std::to_string(rvn++) 
		// Content
		+ Loadout.Skin.PackagePath + "." + Loadout.Skin.Id
		+ " " 
		+ Loadout.Backbling.PackagePath + "." + Loadout.Backbling.Id
		+ " "
		+ Loadout.Pickaxe.PackagePath + "." + Loadout.Pickaxe.Id
	);*/
}

void FLoadout::WriteEmote(const FItem& Emote) {
	static int rvn = 1;

	if (rvn == 9) {
		rvn = 0;
	}

	WindowsFunctionLibrary::StringToImage(
		// Path
		WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath())
		+ "emote.png", std::to_string(rvn++)
		// Content
		+ Emote.PackagePath + "." + Emote.Id
	);
}

void FLoadout::WriteFOV(bool bEnabled) {
	static int rvn = 1;

	if (rvn == 9) {
		rvn = 0;
	}

	WindowsFunctionLibrary::StringToImage(
		// Path
		WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath())
		+ "fov.png", std::to_string(rvn++)
		// Content
		+ (bEnabled ? "true" : "false")
	);
}