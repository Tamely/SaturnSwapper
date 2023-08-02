using System;
using DiscordRPC;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Services;

public sealed class DiscordService
{
    public static DiscordHandler DiscordHandler { get; } = new();
    
    public DiscordService()
    {
        DiscordHandler.Initialize();
    }
}

public class DiscordHandler
{
    private const string APP_ID = "1136322806292942868";

    private RichPresence _currentPresence;
    private readonly DiscordRpcClient _client = new(APP_ID);
    private readonly Timestamps _timestamps = new() { Start = DateTime.UtcNow };

    private readonly Assets _staticAssets = new()
    {
        LargeImageKey = "saturn",
        LargeImageText = $"v{Constants.USER_VERSION}"
    };

    private readonly Button[] _buttons =
    {
        new() { Label = "Join Saturn", Url = "https://discord.io/SaturnSwapper" }
    };

    public void Initialize()
    {
        _currentPresence = new RichPresence()
        {
            Assets = _staticAssets,
            Timestamps = _timestamps,
            Buttons = _buttons,
            Details = $"{Constants.USER_VERSION} - Idling"
        };
        
        _client.OnReady += (_, args) => Logger.Log($"{args.User.Username}#{args.User.Discriminator} ({args.User.ID}) is now ready!");
        _client.SetPresence(_currentPresence);
        _client.Initialize();
    }

    public void UpdatePresence(string details, string state)
    {
        if (!_client.IsInitialized) return;
        _currentPresence.Details = details;
        _currentPresence.State = state;
        _client.SetPresence(_currentPresence);
        _client.Invoke();
    }

    public void UpdateToSavedPresence()
    {
        if (!_client.IsInitialized) return;
        _client.SetPresence(_currentPresence);
        _client.Invoke();
    }

    public void Shutdown()
    {
        if (_client.IsInitialized)
            _client.Deinitialize();
    }
    
    public void Dispose()
    {
        if (!_client.IsDisposed)
            _client.Dispose();
    }
}