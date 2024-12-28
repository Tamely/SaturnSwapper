import Saturn.SaturnFunctionLibrary;

bool SaturnFunctionLibrary::BIND_CALLBACK(ultralight::View* caller, const std::string& jsFunction, JSObjectCallAsFunctionCallback callAsFunction) {
    ultralight::RefPtr<ultralight::JSContext> scopedContext = caller->LockJSContext();
    JSContextRef ctx = (*scopedContext);

    JSStringRef name = JSStringCreateWithUTF8CString(jsFunction.c_str());
    JSObjectRef func = JSObjectMakeFunctionWithCallback(ctx, name, callAsFunction);

    JSObjectRef globalObj = JSContextGetGlobalObject(ctx);
    JSObjectSetProperty(ctx, globalObj, name, func, 0, nullptr);
    JSStringRelease(name);

    return true;
}