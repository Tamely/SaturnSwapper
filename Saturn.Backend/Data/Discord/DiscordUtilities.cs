using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using Saturn.Backend.Data.Discord.Models;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Discord
{
    public class DiscordUtilities
    {
        //Change these values
        private string CLIENT_ID = "1121469600631103551";
        private string CLIENT_SECRET = "VRwwSt6EQsORzb7n_p3SYdsNRc4Q9Fv9";
        private string REDIRECT_URI = "http://localhost:3000/api/auth/callback/discord";
        private string GuildID = "1118556929799761920";
        private static ulong[] TargetRoles = { 8312334953691727223, 14469599625845645863, 15712186866927928291, 16885141162808554974 };
        public static GuildMemberModel? Member = null;
        private TokenResponseModel OAuthToken = null;

        public DiscordWidgetAPIModel serverAPI
        {
            get
            {
                HttpClient client = new HttpClient();
                return JsonConvert.DeserializeObject<DiscordWidgetAPIModel>(client.GetStringAsync($"https://discord.com/api/guilds/{GuildID}/widget.json").GetAwaiter().GetResult());
            }
        }
        public DiscordUtilities()
        {
            //Start local server
            string url = $"https://discord.com/api/oauth2/authorize?client_id={CLIENT_ID}&redirect_uri={REDIRECT_URI}&response_type=code&scope=guilds.members.read";
            Utilities.OpenBrowser(url);

            HttpListener _httpListener = new HttpListener();

            if (!_httpListener.Prefixes.Contains(REDIRECT_URI + "/"))
                _httpListener.Prefixes.Add(REDIRECT_URI + "/");

            if (!_httpListener.IsListening)
                _httpListener.Start();

            //Listen to server until OAuth is filled
            while (OAuthToken == null)
            {
                HttpListenerContext context = _httpListener.GetContext(); // get a context
                if (!string.IsNullOrEmpty(context.Request.Url.Query))
                {
                    NameValueCollection queryDictionary = HttpUtility.ParseQueryString(context.Request.Url.Query);
                    if (queryDictionary["code"] != null)
                    {
                        string DiscordCode = queryDictionary["code"];
                        byte[] _responseArray = Encoding.UTF8.GetBytes($"Discord account data has been sent to the swapper. You may now close this tab"); // get the bytes to response
                        context.Response.OutputStream.Write(_responseArray, 0, _responseArray.Length); // write bytes to the output stream
                        context.Response.KeepAlive = false; // set the KeepAlive bool to false
                        context.Response.Close(); // close the connection

                        Logger.Log("Received Discord Code: " + DiscordCode);
                        Logger.Log("Fetching access_token from Discord Code");
                        OAuthToken = GetToken(DiscordCode);
                    }

                }
            }

            _httpListener.Stop();
            GuildMemberModel CurrentUser = GetGuildMember(OAuthToken.access_token, GuildID);
            if (CurrentUser == null)
                throw new Exception("Discord user not found. Are you in the Saturn+ server?");

            if (CurrentUser.roles.Any(x => TargetRoles.Contains(CityHash.CityHash64(Encoding.UTF8.GetBytes(x)))))
            {
                Member = CurrentUser;
                Constants.isPlus = true;
            }

            Constants.DiscordAvatar = $"https://cdn.discordapp.com/avatars/{CurrentUser.user.id}/{CurrentUser.user.avatar}.png";
            Constants.DiscordName = CurrentUser.user.username;
        }

        private TokenResponseModel GetToken(string code)
        {
            HttpClient client = new();

            var data = new[]
            {
                new KeyValuePair<string, string>("client_id", CLIENT_ID),
                new KeyValuePair<string, string>("client_secret", CLIENT_SECRET),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", REDIRECT_URI),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            };
            var resp = client.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(data)).GetAwaiter().GetResult();
            string stringResp = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return resp.IsSuccessStatusCode ? JsonConvert.DeserializeObject<TokenResponseModel>(stringResp) : null;
        }

        private GuildMemberModel GetGuildMember(string access_Token, string GuildID)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_Token);
            string url = $"https://discord.com/api/users/@me/guilds/{GuildID}/member";
            var resp = client.GetAsync(url).GetAwaiter().GetResult();
            string stringResp = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return resp.IsSuccessStatusCode ? JsonConvert.DeserializeObject<GuildMemberModel>(stringResp) : null;
        }
    }
}
