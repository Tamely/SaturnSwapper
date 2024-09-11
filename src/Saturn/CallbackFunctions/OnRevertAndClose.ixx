module;

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include "Saturn/Defines.h"

export module Saturn.CallbackFunctions.OnRevertAndClose;

import Saturn.Context;
import Saturn.FortniteFunctionLibrary;
import Saturn.Unreal.IoStoreWriter;

import <AppCore/AppCore.h>;

export class FOnRevertAndClose {
public:
	static JSValueRef OnRevertAndClose(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FortniteFunctionLibrary::KillEpicProcesses();
		IoStoreWriter::Revert();
		Sleep(1000);

		exit(0);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnRevertAndClose";
	}
};