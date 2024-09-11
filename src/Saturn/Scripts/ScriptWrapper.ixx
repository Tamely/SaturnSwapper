export module Saturn.Scripts.ScriptWrapper;

import <string>;

export class FScriptWrapper {
public:
	static void InitBindings();
	static void Eval(const std::string& code);
};