using System;

namespace Saturn.Backend.Data.Discord.Models
{
    public class GuildMemberModel
    {
        public string avatar { get; set; }
        public object communication_disabled_until { get; set; }
        public int flags { get; set; }
        public bool is_pending { get; set; }
        public DateTime joined_at { get; set; }
        public string nick { get; set; }
        public bool pending { get; set; }
        public object premium_since { get; set; }
        public string[] roles { get; set; }
        public User user { get; set; }
        public bool mute { get; set; }
        public bool deaf { get; set; }
    }

    public class User
    {
        public string id { get; set; }
        public string username { get; set; }
        public string avatar { get; set; }
        public object avatar_decoration { get; set; }
        public string discriminator { get; set; }
        public int public_flags { get; set; }
    }
}
