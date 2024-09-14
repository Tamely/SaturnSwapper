export module Saturn.Functions.FindPartByPath;

import <duktape/duktape.h>;
import <string>;

export struct FFindPartByPath {
	const static int PART_SIG = 0xAAAD;
	static int PartCount;

	static void encode(const std::string& path);
	// UCustomCharacterPart UFindPartByPath(Path);
	static duk_ret_t dukFindPartByPath(duk_context* ctx);
};