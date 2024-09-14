module;

#include <Crypt/skCrypter.h>

export module Saturn.Discord.RichPresence;

import <DiscordSDK/discord_register.h>;
import <DiscordSDK/discord_rpc.h>;

import <string>;
import <memory>;

DiscordRichPresence m_RichPresence;

export class FRichPresence {
public:
	static void SetUp() {
		DiscordEventHandlers handlers;
		memset(&handlers, 0, sizeof(handlers));
		Discord_Initialize(skCrypt("1121469600631103551"), &handlers, 1, "0");
	}

	static void UpdateDiscord(const std::string& tab = "Launcher") {
		memset(&m_RichPresence, 0, sizeof(m_RichPresence));

		char* buffer = new char[64];
		strcpy_s(buffer, 64, "In the ");
		strcat_s(buffer, 64, tab.c_str());
		strcat_s(buffer, 64, " tab");

		m_RichPresence.state = buffer;
		m_RichPresence.details = "discord.gg/SaturnSwapper";
		m_RichPresence.largeImageKey = "templogo";
		m_RichPresence.largeImageText = "3.0.0";
		m_RichPresence.instance = 1;

		Discord_UpdatePresence(&m_RichPresence);
	}

	static void Initialize() {
		SetUp();
		UpdateDiscord();
	}
};