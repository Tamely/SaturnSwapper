using DiscordRPC;
using DiscordRPC.Message;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Utils;
using System;

namespace Saturn.Backend.Data.Services
{
    public interface IDiscordRPCService
    {
        public DiscordRpcClient Client { get; set; }

        public void UpdatePresence(string details);
    }

    public class DiscordRPCService : IDiscordRPCService
    {
        private readonly Assets _assets = new()
        {
            LargeImageKey = "ph",
            LargeImageText = Constants.UserVersion
        };

        private readonly Button[] _buttons =
        {
            new()
            {
                Label = "Join the Discord",
                Url = "https://discord.gg/qBZ2tUGZ7W"
            },
            new()
            {
                Label = "Check out the Source",
                Url = "https://github.com/Tamely/SaturnSwapper"
            }
        };

        private readonly RichPresence _currentPresence;

        private readonly Timestamps _timestamps = new()
        {
            StartUnixMilliseconds = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        public DiscordRPCService()
        {
            Client = new DiscordRpcClient("909245174260043826");
            Client.OnReady += OnReady;
            Client.OnError += OnError;

            _currentPresence = new RichPresence
            {
                Assets = _assets,
                Buttons = _buttons,
                Timestamps = _timestamps
            };

            Client.Initialize();
        }

        public DiscordRpcClient Client { get; set; }

        public void UpdatePresence(string details)
        {
            if (!Client.IsInitialized)
                return;

            if (Client.CurrentUser != null)
            {
                _currentPresence.Details = details;

                Client.SetPresence(_currentPresence);
                Logger.Log(
                    $"Updated rich presence for {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator:D4}. New presence: {JsonConvert.SerializeObject(_currentPresence)}");
            }
        }

        private void OnReady(object sender, ReadyMessage args)
        {
            Logger.Log($"Initialized rich presence for {args.User.Username}#{args.User.Discriminator:D4}");
            UpdatePresence("Looking at the dashboard");
        }

        private static void OnError(object sender, ErrorMessage args)
        {
            Logger.Log($"Discord RPC error: {args.Type}: {args.Message}", LogLevel.Error);
        }
    }
}