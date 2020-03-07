using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
// ReSharper disable UnusedMember.Global

namespace Selenium.Modules
{
    [Summary("Settings management"), RequireUserPermission(GuildPermission.ManageGuild)]
    public class Settings : SeleniumModuleBase
    {
        public Database Database { get; set; }

        [Command("prefix"), Summary("Set the server prefix")]
        public async Task Prefix(
            [Summary("The new prefix")]
            string prefix)
        {
            var settings = await Database.GetSettings(Context.Guild.Id);
            settings.Prefix = prefix;
            await Database.SetSettings(Context.Guild.Id, settings);

            await Reply(_("settings.prefix.set", Constants.SuccessEmoji, prefix));
        }

        [Command("lang"), Alias("language"), Summary("Lists all languages available")]
        public async Task Language()
        {
            await Reply(_("settings.language.list", Constants.InfoEmoji,
                string.Join(", ", 
                    LocalizationProvider.Instance.LanguageCodes
                        .Select(obj => $"`{obj}`")
                        .ToArray()))
                );
        }

        [Command("lang"), Alias("language"), Summary("Set the server language")]
        public async Task Language(
            [Summary("The language")] string langCode)
        {
            if (!LocalizationProvider.Instance.IsLanguageValid(langCode))
            {
                await Reply(_("settings.language.invalid", Constants.ErrorEmoji, langCode));
                return;
            }

            var settings = await Database.GetSettings(Context.Guild.Id);
            settings.Language = langCode;
            await Database.SetSettings(Context.Guild.Id, settings);

            await Reply(_("settings.language.set", Constants.SuccessEmoji, langCode));
        }

        [Command("automod"), Summary("Automod settings")]
        public async Task Automod()
        {
            var embedTest = new EmbedBuilder()
                .WithTitle($"{Constants.AutomodEmoji} Automod Settings")
                .WithDescription($"{Constants.ErrorEmoji} Automod is not yet available!")
                .Build();

            await Reply(embed: embedTest);
        }
    }
}
