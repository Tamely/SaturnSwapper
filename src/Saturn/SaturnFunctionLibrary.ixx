export module Saturn.SaturnFunctionLibrary;

import <AppCore/AppCore.h>;
import <string>;

export class SaturnFunctionLibrary {
public:
	static bool BIND_CALLBACK(ultralight::View* caller, const std::string& jsFunction, JSObjectCallAsFunctionCallback callAsFunction);
};