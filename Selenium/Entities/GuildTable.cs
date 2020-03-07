using SQLite;

namespace Selenium.Entities
{
    public class GuildTable
    {
        [PrimaryKey]
        public long Id { get; set; }

        public string SettingsJson { get; set; }
    }
}
