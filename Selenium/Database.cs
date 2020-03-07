using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using Selenium.Entities;
using SQLite;

namespace Selenium
{
    public class Database
    {
        private static Database _instance;
        public static Database Instance { get; } = _instance ??= new Database();

        private readonly SQLiteAsyncConnection _connection = new SQLiteAsyncConnection(
            Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetEntryAssembly()?.Location),
                "selenium.db")
            );

        public async Task Initialize()
        {
            await _connection.CreateTableAsync<GuildTable>();
        }

        public async Task SetSettings(ulong guildId, Settings settings)
        {
            var guildSettings = await GetGuildTable(guildId);

            guildSettings.SettingsJson = JsonConvert.SerializeObject(settings);
            await _connection.UpdateAsync(guildSettings);
        }

        public async Task<Settings> GetSettings(ulong guildId)
        {
            var guildData = await GetGuildTable(guildId);

            var settings = JsonConvert.DeserializeObject<Settings>(guildData.SettingsJson);

            return settings;
        }

        public async Task<GuildTable> GetGuildTable(ulong guildId)
        {
            var signedId = Sign(guildId);
            GuildTable guildTable;

            try
            {
                guildTable = await _connection.Table<GuildTable>()
                    .Where(g => g.Id == signedId)
                    .FirstAsync();
            }
            catch (InvalidOperationException)
            {

                guildTable = new GuildTable
                {
                    Id = signedId,
                    SettingsJson = JsonConvert.SerializeObject(new Settings())
                };

                await _connection.InsertAsync(guildTable);
            }

            return guildTable;
        }

        public long Sign(ulong ulongValue)
        {
            return unchecked((long)ulongValue + long.MinValue);
        }

        public ulong Unsign(long longValue)
        {
            return unchecked((ulong)(longValue - long.MinValue));
        }
    }
}
