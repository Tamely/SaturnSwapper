export module Saturn.Functions.ExecuteConsoleCommand;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

export struct FExecuteConsoleCommand {
	static void encode(std::string command) {
		std::string message = std::to_string((int)EOpcodes::EXECUTECONSOLECOMMAND) + ",,,,," + command;
		WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
		std::cout << "Executing command: " << command << std::endl;
	}

	static duk_ret_t dukExecuteConsoleCommand(duk_context* ctx) {
		int ArgsLength = duk_get_top(ctx);
		if (ArgsLength != 1) {
			MessageBoxW(nullptr, L"This function takes 1 argument!", L"ExecuteConsoleCommand", NULL);
			return DUK_RET_TYPE_ERROR;
		}

		std::string cmd = duk_get_string(ctx, 0);

		if (!cmd.empty()) {
			encode(cmd);

			FContext::ResponseWaiting = true;
			while (FContext::ResponseWaiting);
		}
		else {
			MessageBoxW(nullptr, L"Command cannot be empty!", L"ExecuteConsoleCommand", NULL);
			return DUK_RET_TYPE_ERROR;
		}

		return 0;
	}
};