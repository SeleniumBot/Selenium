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

        public string Modlog(SocketUser moderator, SocketUser punished, string method, string emoji, 
            string reason, int caseId, TimeSpan length = default)
        {
            var lengthStr = "";

            if (length != default)
            {
                lengthStr = _("moderation.timespan", formatting: length.ToString(@"d\dhh\hmm\m"));
            }

            return _("moderation.modlog", emoji, 
                DateTime.UtcNow.ToString("HH:mm:ss"),
                caseId,
                $"**{moderator.Username}**#{moderator.Discriminator}",
                _($"moderation.types.{method}"),
                $"**{punished.Username}**#{punished.Discriminator}",
                punished.Id,
                lengthStr,
                reason);
        }
    }
}
