using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Selenium.Modules
{
    public class SeleniumModuleBase : ModuleBase<ShardedCommandContext>
    {
        public Task<IUserMessage> Reply(
            string message = null,
            bool isTTS = false,
            Embed embed = null,
            RequestOptions options = null)
            => ReplyAsync(message, isTTS, embed, options);

#nullable enable
        public string _(string key, string? emoji = null, params object?[] formatting)
#nullable disable
        {
            // TODO: improve
            var lang = Database.Instance.GetSettings(Context.Guild.Id)
                .GetAwaiter().GetResult().Language;

            return LocalizationProvider.Instance._(key, lang, emoji, formatting);
        }
    }
}
