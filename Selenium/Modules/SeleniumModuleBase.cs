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

        /**
         * Function for localizing strings
         */
#nullable enable
        public string _(string key, string? emoji = null, params object?[] formatting)
#nullable disable
        {
            // TODO: fix this somehow
            var settings = Database.Instance.GetSettings(Context.Guild.Id).GetAwaiter().GetResult();
            var trueInput = LocalizationProvider.Instance.GetLocalization(key, settings.Language);
            if (emoji != null)
            {
                trueInput = trueInput.Replace("{E}", emoji);
            }

            return string.Format(trueInput, formatting);
        }
    }
}
