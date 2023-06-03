using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Saturn.Backend.Data.SaturnConfig;
using Saturn.Backend.Data.Services.OobeServiceUtils;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Services;
public class OobeService
{
    private readonly IJSRuntime _jsRuntime;
    public OobeType OobeType;
    
    public OobeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ConfigureOobe()
    {
        OobeType = !File.Exists(Constants.ConfigPath) ? OobeType.NotStarted : Config.Get().GetOobeType();
    }

    public async Task AddFlag(OobeType flag)
    {
        OobeType |= flag;
        Config.Get().SetOobeType(OobeType);
        Config.Get().Dispose();
    }
    
    public async Task Complete()
    {
        await _jsRuntime.InvokeAsync<string>("saturn.oobe.steps.finalIn", null);
        await _jsRuntime.InvokeAsync<string>("saturn.oobe.steps.finalOut", null);
    }
}