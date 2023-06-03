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
using Newtonsoft.Json;
using Saturn.Backend.Data.Discord.Models;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Discord
{
    public class DiscordUtilities
    {
        //Change these values
        private string CLIENT_ID = "1114338834172362893";
        private string CLIENT_SECRET = "bTP0bPjNCbyNyMpp-sLZWu-HCwXSrEDQ";
        private string REDIRECT_URI = "http://localhost:3000/api/auth/callback/discord";
        private string GuildID = "1114156540040056975";
        private string[] TargetRoles = { "1114199040251408394", "1114198787796254842", "1114183776927498270", "1114215451019460682", "1114199157549318218" };
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
                throw new Exception("Null current discord user!");

            if (CurrentUser.roles.Any(x => TargetRoles.Contains(x)))
                Constants.isPlus = true;

            Constants.DiscordAvatar = $"https://cdn.discordapp.com/avatars/{CurrentUser.user.id}/{CurrentUser.user.avatar}.png";
            Constants.DiscordName = CurrentUser.user.username;
            Constants.DiscordDiscriminator = CurrentUser.user.discriminator;
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
