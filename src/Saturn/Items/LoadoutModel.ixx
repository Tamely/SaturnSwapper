export module Saturn.Items.LoadoutModel;

import Saturn.Items.ItemModel;

export struct FLoadout {
	FItem Skin;
	FItem Backbling;
	FItem Pickaxe;

public:
	static void WriteToSaveGame(const FLoadout& Loadout);
	static void WriteEmote(const FItem& Emote);
	static void WriteFOV(bool bEnabled);
};