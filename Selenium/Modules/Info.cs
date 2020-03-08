using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

// ReSharper disable UnusedMember.Global

namespace Selenium.Modules
{
    public class Info : SeleniumModuleBase
    {
        public Config Config { get; set; }

        [Command("ping"), Alias("latency"), Summary("Returns latency to Discord.")]
        public Task Ping()
            => Reply(_("info.ping", Constants.ClockEmoji, Context.Client.Latency + "ms"));

        [Command("version"), Summary("Gets the snapshot version of Selenium.")]
        public Task Version()
            => Reply(_("info.snapshot", Constants.InfoEmoji, Config.Snapshot));

        [Command("shard"), Summary("Gets the shard ID for the guild.")]
        public Task Shard()
            => Reply(_("info.shard", Constants.InfoEmoji, Context.Client.GetShardIdFor(Context.Guild)));

        [Command("testmodlog")]
        public Task TestModlog(SocketUser victim, int id, TimeSpan length,
            [Remainder] string reason)
            => Reply(Modlog(Context.User, victim, "ban", Constants.BanEmoji, reason, id, length));
    }
}
